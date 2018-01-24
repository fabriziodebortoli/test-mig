namespace TaskBuilderNetCore.Documents.Model.Interfaces
{
    public enum ValidationType { SavingData, ValueChanged };

    //====================================================================================    
    public interface IValidator
    {
        ValidationType UsedValidationType { get; }

        bool Validate(IDocument document);
    }
}
