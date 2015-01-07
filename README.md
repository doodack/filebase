#Filebase
##The extremely simple data store

Filebase is a simple library that stores data in JSON files. It is suitable for small projects that do not require a full-fledged database.

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

####bool IsVolatile
Gets or sets a value indicating whether data is the corresponding file is volatile (i.e. can be modified by means other than `FilebaseDataset`.
If it is, a file will be opened and read each time a read operation is performed. Otherwise, data will be cached locally.

##Usage
This section presents the current recommended way of using Filebase. It is likely to change before the library hits the stable stage.

1. Create a class deriving from `FilebaseContext`:

		class MyContext : FilebaseContext
		{
			public MyContext() : base("path\\to\\data\\folder") { }
		}

2. Create and initialize a property in your context for each data set you'd like to use

		class MyContext : FilebaseContext
		{
			public MyContext() : base("path\\to\\data\\folder")
			{
				Foos = new FilebaseDataset<Foo>("foo", this, f => f.FooId);
				Bars = new FilebaseDataset<Bar>("bar", this, f => f.BarId);
			}
			
			public FilebaseDataset<Foo> Foos { get; private set; }
			
			public FilebaseDataset<Bar> Bars { get; private set; }
		}

3. Instantiate or inject the context into your code and you're set!

		var context = new MyContext();
		var foos = context.Foos.GetAll();
