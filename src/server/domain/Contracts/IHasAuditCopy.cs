namespace SilverKinetics.w80.Domain.Contracts;

// Since we do a document replace on Upsert and since only the system (and not user)
// can change some fields (like audit trail, statistics, etc) this interface is called
// to copy these system-only-changable data from current->update before persistence.
// In other cases, we do a partial update (for example, user can update their profile
// fields but not their Role), in which case this interface also handles this scenario.
// TODO:
// Another way to handle this situation is to have a separate embedded
// document just for 'updatable' fields, example:
//  {
//      Main: {},
//      Audit: {},
//      Statistics: {}
//  }
// and then we can just do a normal document update on that embedded document ('Main').
public interface IHasPreUpsertAction<T>
{
    void CopyFrom(T ob);
}