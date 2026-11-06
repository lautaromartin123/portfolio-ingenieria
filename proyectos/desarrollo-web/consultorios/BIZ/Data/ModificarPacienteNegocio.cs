using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data.SqlClient;

namespace BIZ
{
    public class ModificarPacienteNegocio
    {
      public void Modificar(
            Paciente paciente,
            Documento documento)
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


                pacienteNegocio.Modificar(
                    paciente,
                    conexion,
                    transaccion
                );


                documentoNegocio.Modificar(
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
