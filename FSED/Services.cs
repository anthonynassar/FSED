using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSED
{
    public static class Services
    {
        public static void RemoveObjectFromSharingSpace(string connectionString, string objectId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                command.CommandText = "UPDATE objects SET sharingSpaceId = 'NULL' where id = @objectId";
                command.Parameters.AddWithValue("@objectId", objectId);
                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
            }
        }

        public static void UpdateSharingSpace(string connectionString, string sharingSpaceId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                command.CommandText = "UPDATE sharingspaces SET verified = 'true' where id = @sharingSpaceId";
                command.Parameters.AddWithValue("@sharingSpaceId", sharingSpaceId);
                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
            }
        }

        public static string GetConstraints(string connectionString, string sharingSpaceId, string op)
        {
            //string queryString = "SELECT tPatCulIntPatIDPk, tPatSFirstname, tPatSName, tPatDBirthday  FROM  [dbo].[TPatientRaw] WHERE tPatSName = @tPatSName";
            string queryString = "SELECT c.value " +
                "FROM events e " +
                "JOIN sharingspaces ss ON ss.Id = e.sharingspaceid " +
                "JOIN dimensions d ON d.Id = e.dimensionid " +
                "JOIN constraints c ON c.Id = e.constraintid " +
                "WHERE ss.id = @sharingSpaceId AND c.operator = @operator";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(queryString, connection);
                command.Parameters.AddWithValue("@sharingSpaceId", sharingSpaceId);
                command.Parameters.AddWithValue("@operator", op);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                try
                {
                    while (reader.Read())
                    {
                        //Console.WriteLine(String.Format("{0}: {1}", op, reader["value"]));
                        return reader["value"].ToString();
                    }
                }
                finally
                {
                    // Always call Close when done reading.
                    reader.Close();
                }
            }
            return null;
        }
    }
}
