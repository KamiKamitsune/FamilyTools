namespace FamilyTools.EasyCompta.IBusiness;

public interface IBaseBusiness<T>
{
    public Task<T> Create(T t);
    public Task<T?> Find(T t);
    public Task<T?> Find(int id);
    public Task<T> Update(T t);
    public Task<bool> Delete(int id);
}