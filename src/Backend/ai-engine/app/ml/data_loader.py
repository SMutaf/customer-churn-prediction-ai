import pyodbc
import pandas as pd

BASE_CONNECTION_PARTS = (
    r"Server=(localdb)\mssqllocaldb;"
    r"Database=CustomerAiDb;"
    r"Trusted_Connection=yes;"
    r"Encrypt=No;"
    r"TrustServerCertificate=Yes;"
)
DEFAULT_DRIVER = "ODBC Driver 17 for SQL Server"
FALLBACK_DRIVERS = ["ODBC Driver 17 for SQL Server", "SQL Server"]


def build_connection_string(driver=DEFAULT_DRIVER):
    return rf"Driver={{{driver}}};" + BASE_CONNECTION_PARTS


class SqlDataLoader:
    def __init__(self, connection_string=None):
        self.connection_string = connection_string

    def load(self):
        conn = self.connect()
        try:
            customers = pd.read_sql("SELECT * FROM Customers WHERE IsDeleted = 0", conn)
            orders = pd.read_sql("SELECT * FROM Orders WHERE IsDeleted = 0", conn)
            interactions = pd.read_sql("SELECT * FROM Interactions WHERE IsDeleted = 0", conn)
            return customers, orders, interactions
        finally:
            conn.close()

    def connect(self):
        if self.connection_string:
            return pyodbc.connect(self.connection_string)

        last_error = None
        installed_drivers = set(pyodbc.drivers())
        for driver in FALLBACK_DRIVERS:
            if driver not in installed_drivers:
                continue
            try:
                return pyodbc.connect(build_connection_string(driver))
            except pyodbc.Error as error:
                last_error = error

        if last_error:
            raise last_error
        raise RuntimeError("No supported SQL Server ODBC driver was found")
