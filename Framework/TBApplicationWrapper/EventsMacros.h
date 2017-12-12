#pragma once	

//questa macro serve per impostare in automatico il valore Handled
//per mangiare il messaggio di windows quando un evento viene gestito dalla customizzazione
#define MESSAGE_HANDLER_EVENT(eventname, eventarg, description) \
	private: \
	System::EventHandler<eventarg^>^ delegate##eventname; \
	public: \
	[Description(description), LocalizedCategory("Graphics", EBCategories::typeid)] \
	virtual event System::EventHandler<eventarg^>^ eventname \
		{ \
		public: \
			void add (System::EventHandler<eventarg^>^ name)	{ delegate##eventname += name; } \
			void remove(System::EventHandler<eventarg^>^ name)	{ delegate##eventname -= name; } \
			void raise(System::Object^ sender, eventarg^ args)	\
			{ \
				if (delegate##eventname != nullptr && !args->Handled) \
				{ \
					args->Handled = !HasCodeBehind; \
					delegate##eventname(sender, args); \
				} \
			} \
	} 

//questa macro serve per impostare in automatico il valore Handled
//per mangiare il messaggio di windows quando un evento viene gestito dalla customizzazione
#define GENERIC_EVENT(eventname, eventarg) \
	private: \
	System::EventHandler<eventarg^>^ delegate##eventname; \
	public: \
	virtual event System::EventHandler<eventarg^>^ eventname \
		{ \
		public: \
			void add (System::EventHandler<eventarg^>^ name)	{ delegate##eventname += name; } \
			void remove(System::EventHandler<eventarg^>^ name)	{ delegate##eventname -= name; } \
			void raise(System::Object^ sender, eventarg^ args)	\
			{ \
				if (delegate##eventname != nullptr) \
				{ \
					delegate##eventname(sender, args); \
				} \
			} \
	}  

//macro per definire in modo esplicito l'evento (serve per poter clonare gli eventi nel dbt, ad esempio)
#define GENERIC_HANDLER_EVENT(eventname, eventarg, categoryname, categoryclass, description) \
	private: \
	System::EventHandler<eventarg^>^ delegate##eventname; \
	public: \
	[Description(description), LocalizedCategory(categoryname, categoryclass::typeid)]\
	event System::EventHandler<eventarg^>^ eventname \
		{ \
		public: \
			void add (System::EventHandler<eventarg^>^ name)	{ delegate##eventname += name; } \
			void remove(System::EventHandler<eventarg^>^ name)	{ delegate##eventname -= name; } \
			void raise(System::Object^ sender, eventarg^ args)	\
			{ \
				if (delegate##eventname != nullptr) \
				{ \
					delegate##eventname(sender, args); \
				} \
			} \
	}  

#define SIMPLE_EVENT(eventname, eventarg) \
	private: \
	System::EventHandler^ delegate##eventname; \
	public: \
	virtual event System::EventHandler^ eventname \
		{ \
		public: \
			void add (System::EventHandler^ name)	{ delegate##eventname += name; } \
			void remove(System::EventHandler^ name)	{ delegate##eventname -= name; } \
			void raise(System::Object^ sender, eventarg^ args)	\
			{ \
				if (delegate##eventname != nullptr) \
				{ \
					delegate##eventname(sender, args); \
				} \
			} \
	}  