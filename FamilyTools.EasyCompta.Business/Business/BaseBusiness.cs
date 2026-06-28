using FamilyTools.Data.Context;
using FamilyTools.Data.Models.EasyCompta;
using FamilyTools.EasyCompta.IBusiness;

namespace FamilyTools.EasyCompta.Business;

public class BaseBusiness<T>(EasyComptaContext context) : IBaseBusiness<T> where T : BaseModel
{
    protected readonly EasyComptaContext _context = context;

    public virtual async Task<T> Create(T t)
    {
        if (t != null)
        {
            t.Id = default;
            t.CreationDate = DateTime.UtcNow;
            this._context.Set<T>().Add(t);
            await this._context.SaveChangesAsync();
            return t;
        }

        return t;
    }

    public virtual async Task<bool> Delete(int id)
    {
        if (id > -1)
        {
            var t = await this.Find(id);
            if (t != null)
            {
                this._context.Remove(t);
                await this._context.SaveChangesAsync();
                return true;
            }
        }

        return false;
    }

    public virtual async Task<T?> Find(T t)
    {
        if (t != null) return (T?)await this._context.Set<T>().FindAsync(t.Id);
        return default;
    }

    public virtual async Task<T?> Find(int id)
    {
        if (id > -1) return (T?)await this._context.Set<T>().FindAsync(id);
        return default;
    }

    public virtual async Task<T> Update(T t)
    {
        if (t != null)
        {
            var checkT = await this.Find(t);

            if (checkT != null)
            {
                t.UpdateDate = DateTime.UtcNow;
                this._context.Update(t);
                await this._context.SaveChangesAsync();
                return await this.Find(t) ?? default;
            }
        }

        return default;
    }
}