namespace DSLRNet.Core.DAL;

using System.Threading;

public class CsvDataSource<T>(DataSourceConfig paramSource, RandomProvider random, Csv csv) : BaseDataSource<T>(random)
    where T : class, ICloneable<T>, new()
{
    public override Task<IEnumerable<T>> LoadDataAsync()
    {       
        string sourcePath = Path.IsPathRooted(paramSource.SourcePath) ? paramSource.SourcePath : PathHelper.FullyQualifyAppDomainPath(paramSource.SourcePath);
        List<T> list = csv.LoadCsv<T>(sourcePath);
        return Task.FromResult(list.AsEnumerable());
    }
}
