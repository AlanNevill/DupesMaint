using System.Data.SqlClient;

namespace DupesMaintWinForms
{
    class MyDbConnect
    {
        public SqlConnection Cn { get; set; }

        public MyDbConnect(string cnString)
        {
            try
            {
                // Build connection string
                //SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                //builder.ConnectionString = "Data source=(localdb);Integrated Security=true;database=pops";
                //builder.IntegratedSecurity = true;
                //builder.DataSource = @"(localDB)\MSSQLLocalDB";   // update me
                //builder.UserID = "";              // update me
                //builder.Password = "";      // update me
                //builder.InitialCatalog = "pops";

                // Connect to SQL
                SqlConnection connection = new SqlConnection(cnString);
                connection.Open();
                Cn = connection;
            }
            catch (SqlException e)
            {
                throw e;
            }
        }

        public popsDataSet DupesTable ()
        {
            popsDataSet dupsDataSet = new popsDataSet();
            using (var da = new SqlDataAdapter("select * from CheckSumDups order by 2,1", Cn))
            {
                da.Fill(dupsDataSet, "CheckSumDups");
            }

            return dupsDataSet;
        }
    }
}

