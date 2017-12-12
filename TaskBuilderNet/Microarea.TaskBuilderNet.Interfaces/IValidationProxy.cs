namespace Microarea.TaskBuilderNet.Interfaces
{
    /// <summary>
    /// Interfaccia che espone la funzionalita' di recupero
    /// del file xsd da associare alla validazione statica di un'azione
    /// di BO specificata.
    /// </summary>
    //================================================================================
    public interface IValidationProxy
    {
        byte[] GetXsd(bool specificFileRequest, object[] parameters);
    }
}
