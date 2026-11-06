using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data.SqlClient;

namespace BIZ
{
    public class AltaPacienteNegocio
    {
        public void Agregar(Paciente paciente, Documento documento)
        {
            SqlConnection conexion = new SqlConnection(
                ConfigurationManager.ConnectionStrings["Conexion"].ConnectionString
            );

            SqlTransaction transaccion = null;

            try
            {
                conexion.Open();

                transaccion = conexion.BeginTransaction();

                PacienteNegocio pacienteNegocio =
                    new PacienteNegocio();

                DocumentoNegocio documentoNegocio =
                    new DocumentoNegocio();


                int idPaciente = pacienteNegocio.Agregar(
                    paciente,
                    conexion,
                    transaccion
                );


                documento.IdPaciente = idPaciente;

                documentoNegocio.Agregar(
                    documento,
                    conexion,
                    transaccion
                );


                transaccion.Commit();
            }
            catch (Exception)
            {
                if (transaccion != null)
                {
                    transaccion.Rollback();
                }

                throw;
            }
            finally
            {
                conexion.Close();
            }
        }
    }
}
