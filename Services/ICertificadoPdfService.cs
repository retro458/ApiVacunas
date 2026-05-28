namespace ApiVacunas.Services
{
    public interface ICertificadoPdfService
    {
        ////<summary>
        /// Genera el certificado en PDF
        /// </summary>
        ///<param name="idMiembro">ID del miembro titular</param>
        /// <returns>Una tupla con los bytes del archivo y el nombre del miembro, o null si no se encuentra</returns>
        Task<(byte[] PdfBytes, string NombreMiembro)?> GenerarCarnetPdfAsync(int idMiembro);
    }
}