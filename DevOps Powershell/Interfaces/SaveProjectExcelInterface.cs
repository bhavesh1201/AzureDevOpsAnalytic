namespace DevOps_Powershell.Interfaces
{
    public interface ISaveProjectExcel
    {

        Task<string> SaveObjectsToExcelAsync<T>(IEnumerable<T> objects);

    }
}
