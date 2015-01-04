#Filebase
##The extremely simple data store

Filebase is a simple library that stores data in JSON files. It is suitable for small projects that do not require a full-fledged database.

**The library is not ready for production yet. The main reason for this is lack of thread safety.**

##API
###FilebaseContext
####FilebaseContext(string rootPath)
* rootPath: path to the directory where all JSON files are stored

###FilebaseDataset&lt;T&gt;
####FilebaseDataset&lt;T&gt;(string name, FilebaseContext context, Func&lt;T, string&gt; idExtractor)
* name: name of the dataset (used to name the backing file)
* context: the associated context
* idExtractor: function that returns an id from an entity

####GetAll() / GetAllAsync()
Returns a collection of all elements in the set.

####GetById(string id) / GetByIdAsync(string id)
Returns an element with a given id or null if it doesn't exist.

####AddOrUpdate(T record) / AddOrUpdateAsync(T record)
If a record with a given id exists in the store, it updates it. A new record is added otherwise.
Note that it is a client's responsibility to assign an id (i.e. there is no autoincrement as in other databases).

####Delete(string id) / DeleteAsync(string id)
Deletes a record with a given id.