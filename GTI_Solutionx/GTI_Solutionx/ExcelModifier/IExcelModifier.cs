namespace ExcelModifier
{
    interface IExcelExtension
    {
        string path { get; set; }

        void ExcelGenerator();
    }
}
