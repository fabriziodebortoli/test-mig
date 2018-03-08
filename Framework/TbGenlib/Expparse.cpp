
#include "stdafx.h"

#include <math.h>

#include <TbNameSolver\TbNamespaces.h>
#include <TbGeneric\DataObj.h>
#include <TbGeneric\FunctionCall.h>
#include <TbParser\TokensTable.h>

#include "baseapp.h"
#include "expparse.h"
#include "expr.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

//-----------------------------------------------------------------------------
// An expression is stored in a ready-to-evaluate form as a stack of
// items in prefix notation. The hierarchy for ExpItem class is the following:
//
//	ExpItem				an expression item can be ...
//	  |
//	  *---- ExpItemOpe		an operator,
//	  |
//	  *---- ExpItemVrb		a reference to a variable symbol,
//	  |
//	  *---- ExpItemFun		a built-in function call,
//	  |
//	  *---- ExpItemVal		a costant value
//
//-----------------------------------------------------------------------------
// function_table : function, num.parameters, return type, param1, param2 ...
//					negative parameter type indicate exact match (no cast applicable)
//
//-----------------------------------------------------------------------------
#define MAX_FuncParametersTable 10

static const int BASED_CODE function_Table[][MAX_FuncParametersTable] =
{
	{T_FABS,					1, DATA_DBL_TYPE,		DATA_DBL_TYPE,	0,				0},
	{T_FASC,					1, DATA_INT_TYPE,		DATA_STR_TYPE,	0,				0},
	{T_FCDOW,					1, DATA_STR_TYPE,		DATA_DATE_TYPE,	0,				0},
	{T_FCEIL,					1, DATA_DBL_TYPE,		DATA_DBL_TYPE,	0,				0},
	{T_FCHR,					1, DATA_STR_TYPE,		DATA_INT_TYPE,	0,				0},
	{T_FCMONTH,					1, DATA_STR_TYPE,		DATA_DATE_TYPE,	0,				0},
	{T_FCTOD,					1, DATA_DATE_TYPE,		DATA_STR_TYPE,	0,				0},
	{T_FDATE,					0, DATA_DATE_TYPE,		0,				0,				0},
	{T_FDATETIME,				0, DATA_DATE_TYPE,		0,				0,				0},
	{T_FDAY,					1, DATA_INT_TYPE,		DATA_DATE_TYPE,	0,				0},
	{T_FDTOC,					1, DATA_STR_TYPE,		DATA_DATE_TYPE,	0,				0},
	{T_FELAPSED_TIME,			0, DATA_LNG_TYPE,		0,				0,				0},	//@@ElapsedTime
	{T_FFIND,					2, DATA_INT_TYPE,		DATA_STR_TYPE,	DATA_STR_TYPE,	0},
	{T_FREVERSEFIND,			2, DATA_INT_TYPE,		DATA_STR_TYPE,	DATA_STR_TYPE,	0},
	{T_FFLOOR,					1, DATA_DBL_TYPE,		DATA_DBL_TYPE,	0,				0},
	{T_FLEFT,					2, DATA_STR_TYPE,		DATA_STR_TYPE,	DATA_INT_TYPE,	0},
	{T_FLEN,					1, DATA_INT_TYPE,		DATA_STR_TYPE,	0,				0},
	{T_FLOWER,					1, DATA_STR_TYPE,		DATA_STR_TYPE,	0,				0},
	{T_FLTRIM,					1, DATA_STR_TYPE,		DATA_STR_TYPE,	0,				0},
	{T_FMAX,					2, DATA_VARIANT_TYPE,	DATA_VARIANT_TYPE,	0,	0},
	{T_FMIN,					2, DATA_VARIANT_TYPE,	DATA_VARIANT_TYPE,	0,	0},
	{T_FMOD,					2, DATA_DBL_TYPE,		DATA_DBL_TYPE,	DATA_DBL_TYPE,	0},
	{T_FMONTH,					1, DATA_INT_TYPE,		DATA_DATE_TYPE,	0,				0},
	{T_FREMOVENEWLINE,			1, DATA_STR_TYPE,		DATA_STR_TYPE,	0,	0},
	{T_FREPLACE,				3, DATA_STR_TYPE,		DATA_STR_TYPE,	DATA_STR_TYPE,	DATA_STR_TYPE},
	{T_FRIGHT,					2, DATA_STR_TYPE,		DATA_STR_TYPE,	DATA_INT_TYPE,	0},
	{T_FROUND,					1, DATA_DBL_TYPE,		DATA_DBL_TYPE,	0,				0},
	{T_FRTRIM,					1, DATA_STR_TYPE,		DATA_STR_TYPE,	0,				0},
	{T_FSIGN,					1, DATA_INT_TYPE,		DATA_DBL_TYPE,	0,				0},
	{T_FSPACE,					1, DATA_STR_TYPE,		DATA_INT_TYPE,	0,				0},
	{T_FSTR,					2, DATA_STR_TYPE,		DATA_DBL_TYPE,	DATA_INT_TYPE,	0},
	{T_FSUBSTR,					2, DATA_STR_TYPE,		DATA_STR_TYPE,	DATA_INT_TYPE,	0},
	{T_FSUBSTRWW,				3, DATA_STR_TYPE,		DATA_STR_TYPE,	DATA_INT_TYPE,	DATA_INT_TYPE},
	{T_FTIME,					0, DATA_DATE_TYPE,		0,				0,				0},
	{T_FTRIM,					1, DATA_STR_TYPE,		DATA_STR_TYPE,	0,				0},
	{T_FUPPER,					1, DATA_STR_TYPE,		DATA_STR_TYPE,	0,				0},
	{T_FVAL,					1, DATA_DBL_TYPE,		DATA_STR_TYPE,	0,				0},
	{T_FYEAR,					1, DATA_INT_TYPE,		DATA_DATE_TYPE,	0,				0},
	{T_FINT,					1, DATA_INT_TYPE,		DATA_DBL_TYPE,	0,				0},
	{T_FLONG,					1, DATA_LNG_TYPE,		DATA_DBL_TYPE,	0,				0},
	{T_FMONTH_DAYS,				2, DATA_INT_TYPE,		DATA_INT_TYPE,	DATA_INT_TYPE,	0},
	{T_FMONTH_NAME,				1, DATA_STR_TYPE,		DATA_INT_TYPE,	0,				0},
	{T_FLAST_MONTH_DAY,			1, DATA_DATE_TYPE,		DATA_DATE_TYPE,	0,				0},
	{T_FFORMAT,					1, DATA_STR_TYPE,		DATA_VARIANT_TYPE,	0,				0},
	{T_FAPP_DATE,				0, DATA_DATE_TYPE,		0,				0,				0},
	{T_FAPP_YEAR,				0, DATA_INT_TYPE,		0,				0,				0},
	{T_FTYPED_BARCODE,			2, DATA_STR_TYPE,		DATA_STR_TYPE,	DATA_INT_TYPE,	0},
	{T_FGETBARCODE_ID,			1, DATA_INT_TYPE,		DATA_ENUM_TYPE,	0,				0},
	{T_FDAYOFWEEK,				1, DATA_INT_TYPE,		DATA_DATE_TYPE,	0,				0},
	{T_FWEEKOFMONTH,			1, DATA_INT_TYPE,		DATA_DATE_TYPE,	0,				0},
	{T_FWEEKOFYEAR,				1, DATA_INT_TYPE,		DATA_DATE_TYPE,	0,				0},
	{T_FGIULIANDATE,			1, DATA_LNG_TYPE,		DATA_DATE_TYPE,	0,				0},
	{T_FDAYOFYEAR,				1, DATA_INT_TYPE,		DATA_DATE_TYPE,	0,				0},
	{T_FLOADTEXT,				1, DATA_STR_TYPE,		DATA_STR_TYPE,	0,				0},
	{T_FSAVETEXT,				2, DATA_BOOL_TYPE,		DATA_STR_TYPE,	DATA_STR_TYPE,				0},
	{T_FLOCALIZE,				1, DATA_STR_TYPE,		DATA_STR_TYPE,	0,				0},
	{T_FISACTIVATED,			2, DATA_BOOL_TYPE,		DATA_STR_TYPE,	DATA_STR_TYPE	},
	{T_FISADMIN,				0, DATA_BOOL_TYPE,		0	},
	{T_FRGB,					3, DATA_LNG_TYPE,		DATA_INT_TYPE,	DATA_INT_TYPE,	DATA_INT_TYPE},

	{T_FGETAPPTITLE_FROM_NS,	1, DATA_STR_TYPE,		DATA_STR_TYPE	},
	{T_FGETMODTITLE_FROM_NS,	1, DATA_STR_TYPE,		DATA_STR_TYPE	},
	{T_FGETDOCTITLE_FROM_NS,	1, DATA_STR_TYPE,		DATA_STR_TYPE	},
	{T_FGETPATH_FROM_NS,		1, DATA_STR_TYPE,		DATA_STR_TYPE	},
	{T_FGETNS_FROM_PATH,		1, DATA_STR_TYPE,		DATA_STR_TYPE	},

	{T_FGETDATABASETYPE,		0, DATA_STR_TYPE		},
	{T_FGETEDITION,				0, DATA_STR_TYPE		},
	{T_FGETCOMPUTERNAME,		0, DATA_STR_TYPE		},
	{T_FGETCOMPANYNAME,			0, DATA_STR_TYPE		},
	{T_FGETINSTALLATIONNAME,	0, DATA_STR_TYPE		},
	{T_FGETINSTALLATIONPATH,	0, DATA_STR_TYPE		},
	{T_FGETINSTALLATIONVERSION,	0, DATA_STR_TYPE		},
	{T_FGETNEWGUID,				0, DATA_GUID_TYPE		},
	{T_FGETPRODUCTLANGUAGE,		0, DATA_STR_TYPE		},
	{T_FGETLOGINNAME,			0, DATA_STR_TYPE		},
	{T_FGETUSERDESCRIPTION, 	0, DATA_STR_TYPE		},
	{T_FGETWINDOWUSER,			0, DATA_STR_TYPE		},

	{T_FGETCULTURE,				0, DATA_STR_TYPE		},
	{T_FSETCULTURE,				1, DATA_STR_TYPE,		DATA_STR_TYPE },

	{T_FMAKELOWERLIMIT,			1, DATA_STR_TYPE,		DATA_STR_TYPE },
	{T_FMAKEUPPERLIMIT,			1, DATA_STR_TYPE,		DATA_STR_TYPE },
	{T_FGETUPPERLIMIT,			1, DATA_STR_TYPE,		DATA_INT_TYPE },

	{T_FCONTENTOF,				1, DATA_BOOL_TYPE,		DATA_STR_TYPE },	 //fittizia, usata in Woorm nelle Table Rule 
	{T_FVALUEOF,				1, DATA_STR_TYPE,		DATA_VARIANT_TYPE }, //fittizia, usata in Woorm nelle Table Rule

	{T_FARRAY_ATTACH,			2, DATA_BOOL_TYPE,		DATA_ARRAY_TYPE,		DATA_ARRAY_TYPE },
	{T_FARRAY_DETACH,			1, DATA_BOOL_TYPE,		DATA_ARRAY_TYPE },
	{T_FARRAY_CLEAR,			1, DATA_LNG_TYPE,		DATA_ARRAY_TYPE },
	{T_FARRAY_COPY,				2, DATA_BOOL_TYPE,		DATA_ARRAY_TYPE,	DATA_ARRAY_TYPE },
	{T_FARRAY_FIND,				2, DATA_LNG_TYPE,		DATA_ARRAY_TYPE,	DATA_VARIANT_TYPE/*Legal parameter Data-Type depends on the first parameter Base Data-Type*/ },
	{T_FARRAY_GETAT,			2, DATA_VARIANT_TYPE/*Return Data-Type depends on the first parameter Base Data-Type*/,		DATA_ARRAY_TYPE,	DATA_LNG_TYPE },
	{T_FARRAY_SETAT,			3, DATA_VARIANT_TYPE/*see previous GetAt*/,		DATA_ARRAY_TYPE,	DATA_LNG_TYPE,	DATA_VARIANT_TYPE/*Legal parameter Data-Type depends on the first parameter Base Data-Type*/ },
	{T_FARRAY_SIZE,				1, DATA_LNG_TYPE,		DATA_ARRAY_TYPE },
	{T_FARRAY_SORT,				1, DATA_BOOL_TYPE,		DATA_ARRAY_TYPE },
	{T_FARRAY_APPEND,			2, DATA_BOOL_TYPE,		DATA_ARRAY_TYPE,	DATA_ARRAY_TYPE },
	{T_FARRAY_ADD,				2, DATA_BOOL_TYPE,		DATA_ARRAY_TYPE,	DATA_VARIANT_TYPE },
	{T_FARRAY_INSERT,			3, DATA_BOOL_TYPE,		DATA_ARRAY_TYPE,	DATA_LNG_TYPE,		DATA_VARIANT_TYPE },
	{T_FARRAY_REMOVE,			2, DATA_VARIANT_TYPE,	DATA_ARRAY_TYPE,	DATA_LNG_TYPE },
	{T_FARRAY_CONTAINS,			2, DATA_BOOL_TYPE,		DATA_ARRAY_TYPE,	DATA_VARIANT_TYPE },
	{T_FARRAY_CREATE,			1, DATA_ARRAY_TYPE,		DATA_VARIANT_TYPE },
	{T_FARRAY_SUM,				1, DATA_VARIANT_TYPE,	DATA_ARRAY_TYPE },

	{T_FISDATABASEUNICODE,		0, DATA_BOOL_TYPE		},
	{T_FTABLEEXISTS,			1, DATA_BOOL_TYPE,		DATA_STR_TYPE },
	{T_FFILEEXISTS,				1, DATA_BOOL_TYPE,		DATA_STR_TYPE },
	
	{T_FGETSETTING,				4, DATA_VARIANT_TYPE/*Return Data-Type depends on the forth parameter Data-Type*/,		DATA_STR_TYPE,		DATA_STR_TYPE,	DATA_STR_TYPE,	DATA_VARIANT_TYPE },
	{T_FSETSETTING,				4, DATA_VARIANT_TYPE/*Return Data-Type depends on the forth parameter Data-Type*/,		DATA_STR_TYPE,		DATA_STR_TYPE,	DATA_STR_TYPE,	DATA_VARIANT_TYPE },
	
	{T_FISREMOTEINTERFACE,		0, DATA_BOOL_TYPE		},
	{T_FISRUNNINGFROMEXTERNALCONTROLLER,		0, DATA_BOOL_TYPE		},
	{T_FISWEB,					0, DATA_BOOL_TYPE		},

	{T_FDECODE,					3, DATA_VARIANT_TYPE,	DATA_VARIANT_TYPE,	DATA_VARIANT_TYPE,	DATA_VARIANT_TYPE },
	{T_FCHOOSE,					2, DATA_VARIANT_TYPE,	DATA_INT_TYPE,	DATA_VARIANT_TYPE },
	{T_FIIF,					3, DATA_VARIANT_TYPE,	DATA_BOOL_TYPE,	DATA_VARIANT_TYPE,	DATA_VARIANT_TYPE },

	{T_FWILDCARD_MATCH,			2, DATA_BOOL_TYPE,		DATA_STR_TYPE,	DATA_STR_TYPE},
	
	{T_FISEMPTY,				1, DATA_BOOL_TYPE,		DATA_VARIANT_TYPE },
	{T_FISNULL,					2, DATA_VARIANT_TYPE,	DATA_VARIANT_TYPE, DATA_VARIANT_TYPE },

	{T_FDATEADD,				4, DATA_DATE_TYPE,		DATA_DATE_TYPE,		DATA_INT_TYPE,		DATA_INT_TYPE,		DATA_INT_TYPE },
	{T_FWEEKSTARTDATE,			2, DATA_DATE_TYPE,		DATA_INT_TYPE,		DATA_INT_TYPE },
	{T_FISLEAPYEAR	,			1, DATA_BOOL_TYPE,		DATA_INT_TYPE },
	{T_FEASTERSUNDAY,			1, DATA_DATE_TYPE,		DATA_INT_TYPE },

	{T_FREPLICATE,				2, DATA_STR_TYPE,		DATA_STR_TYPE,		DATA_INT_TYPE,	0,				0 },
	{T_FPADLEFT,				3, DATA_STR_TYPE,		DATA_STR_TYPE,		DATA_INT_TYPE,	DATA_STR_TYPE,		0 },
	{T_FPADRIGHT,				3, DATA_STR_TYPE,		DATA_STR_TYPE,		DATA_INT_TYPE,	DATA_STR_TYPE,		0 },
	{T_FCOMPARE_NO_CASE,		2, DATA_INT_TYPE,		DATA_STR_TYPE,		DATA_STR_TYPE },

	{T_FSEND_BALLOON,			7, DATA_BOOL_TYPE,		DATA_STR_TYPE,		DATA_STR_TYPE,		DATA_INT_TYPE,		DATA_DATE_TYPE,		DATA_BOOL_TYPE,		DATA_BOOL_TYPE,		DATA_LNG_TYPE },
	{T_FFORMAT_TBLINK,			3, DATA_STR_TYPE,		DATA_STR_TYPE,		DATA_STR_TYPE,		DATA_VARIANT_TYPE },

	{T_FEXECUTESCRIPT,			1, DATA_BOOL_TYPE,		DATA_STR_TYPE },

	{T_FGETTITLE,				1, DATA_STR_TYPE,		DATA_VARIANT_TYPE },	//Usata dagli hotkeylink XML nel MailConector

// Prototipi funzionanti ma non utilizzati
	{T_FCONVERT,				2, DATA_VARIANT_TYPE/*Return Data-Type depends on the first parameter Data-Type name*/,	DATA_VARIANT_TYPE,	DATA_STR_TYPE },
	{T_FTYPEOF,					1, DATA_STR_TYPE,		DATA_VARIANT_TYPE },
	{T_FPREV_VALUE,				1, DATA_VARIANT_TYPE,	DATA_VARIANT_TYPE },
	{T_FNEXT_VALUE,				1, DATA_VARIANT_TYPE,	DATA_VARIANT_TYPE },
	{T_FADDRESSOF,				2, DATA_LNG_TYPE,		DATA_STR_TYPE,		DATA_VARIANT_TYPE },

	{T_FGETTHREADCONTEXT,		2, DATA_BOOL_TYPE,		DATA_STR_TYPE, DATA_VARIANT_TYPE },
	{T_FOWNTHREADCONTEXT,		1, DATA_BOOL_TYPE,		DATA_VARIANT_TYPE },
	
	{T_FRECORD_GETFIELD,		2, DATA_VARIANT_TYPE,	DATA_RECORD_TYPE,		DATA_STR_TYPE },
	{T_FSQLRECORD_GETFIELD,		2, DATA_VARIANT_TYPE,	DATA_SQLRECORD_TYPE,	DATA_STR_TYPE },
	{T_FOBJECT_GETFIELD,		2, DATA_VARIANT_TYPE,	DATA_LNG_TYPE,			DATA_STR_TYPE },

	{T_FCOLUMN_GETAT,			2, DATA_VARIANT_TYPE,	DATA_VARIANT_TYPE,	DATA_LNG_TYPE },
	{T_FCOLUMN_FIND,			2, DATA_LNG_TYPE,		DATA_VARIANT_TYPE,	DATA_VARIANT_TYPE/*Legal parameter Data-Type depends on the first parameter Base Data-Type*/ },
	{T_FCOLUMN_SIZE,			1, DATA_LNG_TYPE,		DATA_VARIANT_TYPE },
	{T_FCOLUMN_SUM,				1, DATA_VARIANT_TYPE,	DATA_VARIANT_TYPE },

	{TT_NULL,					0 }
};

//-----------------------------------------------------------------------------
// optFuncParam_table : function, optional num.parameters, type param1, param2 ...
//-----------------------------------------------------------------------------
#define MAX_optFuncParametersTable 8

static const int BASED_CODE optFuncParam_Table[][MAX_optFuncParametersTable]=
{
	{T_FSTR,					1,	DATA_INT_TYPE	},
	{T_FDATE,					3,	DATA_INT_TYPE,	DATA_INT_TYPE,	DATA_INT_TYPE	},
	{T_FTIME,					3,	DATA_INT_TYPE,	DATA_INT_TYPE,	DATA_INT_TYPE	},
	{T_FDATETIME,				6,	DATA_INT_TYPE,	DATA_INT_TYPE,	DATA_INT_TYPE, DATA_INT_TYPE, DATA_INT_TYPE, DATA_INT_TYPE	},
	{T_FELAPSED_TIME,			2,	DATA_DATE_TYPE,	DATA_DATE_TYPE	},	//@@ElapsedTime
	{T_FFORMAT,					1,	DATA_STR_TYPE	},
	{T_FSUBSTR,					1,	DATA_INT_TYPE	},
	{T_FFIND,					2,	DATA_INT_TYPE,	DATA_INT_TYPE	},
	{T_FREVERSEFIND,			2,	DATA_INT_TYPE,	DATA_INT_TYPE	},
	{T_FROUND,					1,	DATA_INT_TYPE	},
	{T_FTABLEEXISTS,			1,	DATA_STR_TYPE	},
	{T_FARRAY_SORT,				3,	DATA_BOOL_TYPE, DATA_LNG_TYPE , DATA_LNG_TYPE },
	{T_FARRAY_FIND,				1,	DATA_LNG_TYPE	},
	{T_FGETCULTURE,				2,  DATA_STR_TYPE, DATA_BOOL_TYPE },
	{T_FGETPATH_FROM_NS,		1,	DATA_STR_TYPE	},
	{T_FGETUSERDESCRIPTION,		1,  DATA_STR_TYPE	},
	{T_FUPPER,					1,  DATA_BOOL_TYPE	},
	{T_FLOWER,					1,  DATA_BOOL_TYPE	},
	{T_FGETCOMPUTERNAME,		1,  DATA_BOOL_TYPE	},
	{T_FWEEKOFMONTH,			1,  DATA_INT_TYPE	},
	{T_FTYPED_BARCODE,			6,  DATA_INT_TYPE, DATA_STR_TYPE,  DATA_INT_TYPE,  DATA_INT_TYPE, DATA_STR_TYPE,  DATA_INT_TYPE },
	{T_FGETBARCODE_ID,			1,  DATA_STR_TYPE	},
	{T_FSAVETEXT,				1,  DATA_INT_TYPE	},
	{T_FDATEADD,				3,	DATA_INT_TYPE,	DATA_INT_TYPE,	DATA_INT_TYPE },

	{T_FTRIM,					2,  DATA_STR_TYPE,  DATA_BOOL_TYPE },
	{T_FLTRIM,					1,  DATA_STR_TYPE },
	{T_FRTRIM,					1,  DATA_STR_TYPE },

	{T_FARRAY_CREATE,			255, DATA_VARIANT_TYPE },

	{T_FDECODE,					255, DATA_VARIANT_TYPE },
	{T_FCHOOSE,					255, DATA_VARIANT_TYPE },
	{T_FIIF,					255, DATA_VARIANT_TYPE },

	{T_FMIN,					255, DATA_VARIANT_TYPE },
	{T_FMAX,					255, DATA_VARIANT_TYPE },

	{T_FFORMAT_TBLINK,			255, DATA_VARIANT_TYPE },

	//non sono gestiti overloading con uno stesso numero di parametri
	//{T_FGETUSERDESCRIPTION,		1,  DATA_LNG_TYPE	},

	{TT_NULL,					0	}
};

//-----------------------------------------------------------------------

static const int BASED_CODE function_Table_Group[][2] =
{
	{ G_DATETIME, T_FCMONTH				},
	{ G_DATETIME, T_FCTOD				},
	{ G_DATETIME, T_FDATE				},
	{ G_DATETIME, T_FDATETIME			},
	{ G_DATETIME, T_FDAY				},
	{ G_DATETIME, T_FDTOC				},
	{ G_DATETIME, T_FELAPSED_TIME		},
	{ G_DATETIME, T_FCDOW				},
	{ G_DATETIME, T_FMONTH				},
	{ G_DATETIME, T_FTIME				},
	{ G_DATETIME, T_FYEAR				},
	{ G_DATETIME, T_FMONTH_DAYS			},
	{ G_DATETIME, T_FMONTH_NAME			},
	{ G_DATETIME, T_FLAST_MONTH_DAY		},
	{ G_DATETIME, T_FDAYOFWEEK			},
	{ G_DATETIME, T_FWEEKOFMONTH		},
	{ G_DATETIME, T_FWEEKOFYEAR			},
	{ G_DATETIME, T_FGIULIANDATE		},
	{ G_DATETIME, T_FDAYOFYEAR			},
	{ G_DATETIME, T_FAPP_DATE			},
	{ G_DATETIME, T_FAPP_YEAR			},
	{ G_DATETIME, T_FDATEADD },
	{ G_DATETIME, T_FWEEKSTARTDATE },
	{ G_DATETIME, T_FISLEAPYEAR },
	{ G_DATETIME, T_FEASTERSUNDAY },

	{ G_NUMERIC, T_FABS					},
	{ G_NUMERIC, T_FCEIL				},
	{ G_NUMERIC, T_FFLOOR				},
	{ G_NUMERIC, T_FMOD					},
	{ G_NUMERIC, T_FROUND				},
	{ G_NUMERIC, T_FSIGN				},
	{ G_NUMERIC, T_FINT					},
	{ G_NUMERIC, T_FLONG				},
	{ G_NUMERIC, T_FVAL					},

	{ G_STRING, T_FASC					},
	{ G_STRING, T_FCHR					},
	{ G_STRING, T_FFIND					},
	{ G_STRING, T_FREVERSEFIND			},
	{ G_STRING, T_FLEFT					},
	{ G_STRING, T_FLEN					},
	{ G_STRING, T_FLOWER				},
	{ G_STRING, T_FLTRIM				},
	{ G_STRING, T_FREMOVENEWLINE		},
	{ G_STRING, T_FREPLACE				},
	{ G_STRING, T_FRIGHT				},
	{ G_STRING, T_FRTRIM				},
	{ G_STRING, T_FSPACE				},
	{ G_STRING, T_FSTR					},
	{ G_STRING, T_FSUBSTR				},
	{ G_STRING, T_FSUBSTRWW				},
	{ G_STRING, T_FTRIM					},
	{ G_STRING, T_FUPPER				},
	{ G_STRING, T_FMAKELOWERLIMIT		},
	{ G_STRING, T_FMAKEUPPERLIMIT		},
	{ G_STRING, T_FGETUPPERLIMIT		},
	{ G_STRING, T_FFORMAT				},
	{ G_STRING, T_FLOADTEXT				},
	{ G_STRING, T_FSAVETEXT				},
	{ G_STRING, T_FLOCALIZE				},
	{ G_STRING, T_FWILDCARD_MATCH		},

	{ G_STRING, T_FREPLICATE			},
	{ G_STRING, T_FPADLEFT				},
	{ G_STRING, T_FPADRIGHT				},
	{ G_STRING, T_FCOMPARE_NO_CASE		},

	{ G_SYSTEM, T_FISACTIVATED			},
	{ G_SYSTEM, T_FISADMIN				},
	{ G_SYSTEM, T_FGETAPPTITLE_FROM_NS	},
	{ G_SYSTEM, T_FGETMODTITLE_FROM_NS	},
	{ G_SYSTEM, T_FGETDOCTITLE_FROM_NS	},
	{ G_SYSTEM, T_FGETPATH_FROM_NS		},
	{ G_SYSTEM, T_FGETNS_FROM_PATH		},
	{ G_SYSTEM, T_FGETDATABASETYPE		},
	{ G_SYSTEM, T_FGETEDITION },
	{ G_SYSTEM, T_FGETCOMPUTERNAME },
	{ G_SYSTEM, T_FGETCOMPANYNAME },
	{ G_SYSTEM, T_FGETINSTALLATIONNAME },
	{ G_SYSTEM, T_FGETINSTALLATIONPATH },
	{ G_SYSTEM, T_FGETINSTALLATIONVERSION },
	{ G_SYSTEM, T_FGETNEWGUID },
	{ G_SYSTEM, T_FGETPRODUCTLANGUAGE },
	{ G_SYSTEM, T_FGETLOGINNAME },
	{ G_SYSTEM, T_FGETUSERDESCRIPTION },
	{ G_SYSTEM, T_FGETWINDOWUSER },
	{ G_SYSTEM, T_FGETCULTURE },
	{ G_SYSTEM, T_FSETCULTURE },
	{ G_SYSTEM, T_FISDATABASEUNICODE },		
	{ G_SYSTEM, T_FISREMOTEINTERFACE },
	{ G_SYSTEM, T_FISRUNNINGFROMEXTERNALCONTROLLER },
	{ G_SYSTEM, T_FISWEB },

	{ G_OTHER, T_FMAX					},
	{ G_OTHER, T_FMIN					},
	{ G_OTHER, T_FTYPED_BARCODE			},
	{ G_OTHER, T_FGETBARCODE_ID			},
	{ G_OTHER, T_FRGB					},
	{ G_OTHER, T_FCONTENTOF },	  
	{ G_OTHER, T_FVALUEOF },  
	{ G_OTHER, T_FTABLEEXISTS		 },
	{ G_OTHER, T_FFILEEXISTS		 },
	{ G_OTHER, T_FGETSETTING },
	{ G_OTHER, T_FSETSETTING },
	{ G_OTHER, T_FDECODE },
	{ G_OTHER, T_FISEMPTY },
	{ G_OTHER, T_FISNULL },
	{ G_OTHER, T_FSEND_BALLOON },
	{ G_OTHER, T_FFORMAT_TBLINK },
	{ G_OTHER, T_FEXECUTESCRIPT },

	{ G_ARRAY, T_FARRAY_CREATE  },
	{ G_ARRAY, T_FARRAY_ATTACH  },
	{ G_ARRAY, T_FARRAY_DETACH },
	{ G_ARRAY, T_FARRAY_CLEAR },
	{ G_ARRAY, T_FARRAY_COPY },
	{ G_ARRAY, T_FARRAY_FIND },
	{ G_ARRAY, T_FARRAY_GETAT },
	{ G_ARRAY, T_FARRAY_SETAT },
	{ G_ARRAY, T_FARRAY_SIZE },
	{ G_ARRAY, T_FARRAY_SORT },
	{ G_ARRAY, T_FARRAY_APPEND  },
	{ G_ARRAY, T_FARRAY_ADD  },
	{ G_ARRAY, T_FARRAY_INSERT  },
	{ G_ARRAY, T_FARRAY_REMOVE  },
	{ G_ARRAY, T_FARRAY_CONTAINS  },
	{ G_ARRAY, T_FARRAY_SUM  },

	{ G_PRIVATE, T_FGETTITLE },	
	{ G_PRIVATE, T_FPREV_VALUE },
	{ G_PRIVATE, T_FNEXT_VALUE },
	{ G_PRIVATE, T_FCONVERT },
	{ G_PRIVATE, T_FTYPEOF },
	{ G_PRIVATE, T_FADDRESSOF },

	{ G_PRIVATE, T_FGETTHREADCONTEXT },
	{ G_PRIVATE, T_FOWNTHREADCONTEXT },

	{ G_PRIVATE, T_FRECORD_GETFIELD },
	{ G_PRIVATE, T_FSQLRECORD_GETFIELD },
	{ G_PRIVATE, T_FOBJECT_GETFIELD },

	{ G_PRIVATE, T_FCOLUMN_GETAT },
	{ G_PRIVATE, T_FCOLUMN_FIND },
	{ G_PRIVATE, T_FCOLUMN_SIZE },
	{ G_PRIVATE, T_FCOLUMN_SUM },

	{ G_PRIVATE, TT_NULL }
};

//-----------------------------------------------------------------------------
CString return_func_group_description(EGroupFunction nGroup)
{
	switch (nGroup)
	{
	case EGroupFunction::G_PRIVATE:
		return _TB("Private");
	case EGroupFunction::G_DATETIME:
		return _TB("Date-Time");
	case EGroupFunction::G_NUMERIC:
		return _TB("Numeric");
	case EGroupFunction::G_STRING:
		return _TB("String");
	case EGroupFunction::G_OTHER:
		return _TB("Miscellaneous");
	case EGroupFunction::G_SYSTEM:
		return _TB("System");
	case EGroupFunction::G_ARRAY:
		return _TB("Array");
	default:
		ASSERT(FALSE);
	}
	return _T("");
}

//-----------------------------------------------------------------------------
EGroupFunction return_func_group(int nFun)
{
	for (int i = 0; (function_Table_Group[i][1] != TT_NULL); i++)
		if (nFun == function_Table_Group[i][1])
		{
			return (EGroupFunction) function_Table_Group[i][0];
		}

	return EGroupFunction::G_PRIVATE;
}

//-----------------------------------------------------------------------------
BOOL return_func_type(int nFun, DataType& c)
{
	for (int i=0; (function_Table[i][0] != TT_NULL); i++)
		if (nFun == function_Table[i][0])
		{
			c = DataType((WORD)function_Table[i][2], 0);
			return TRUE;
		}

	return FALSE;
}

//-----------------------------------------------------------------------------
int parameters_of(int nFun)
{
	for (int i = 0; (function_Table[i][0] != TT_NULL); i++)
		if (nFun == function_Table[i][0])
			return function_Table[i][1];

	return -1;
}

//-----------------------------------------------------------------------------
int opt_parameters_of(int nFun)
{
	for (int i = 0; (optFuncParam_Table[i][0] != TT_NULL); i++)
		if (nFun == optFuncParam_Table[i][0])
			return optFuncParam_Table[i][1];

	return -1;
}

//-----------------------------------------------------------------------------
DataType param_fun(int nFun, int wIdParam)
{
	ASSERT((wIdParam + 2) < MAX_FuncParametersTable);

	for (int i = 0; (function_Table[i][0] != TT_NULL); i++)
		if (function_Table[i][0] == nFun)
		{
			return DataType((WORD)function_Table[i][wIdParam + 2], 0);
		}
	return DataType::Null;
}

//-----------------------------------------------------------------------------
DataType opt_param_fun(int nFun, int wIdParam)
{
	for (int i = 0; (optFuncParam_Table[i][0] != TT_NULL); i++)
		if (optFuncParam_Table[i][0] == nFun)
		{
			if (optFuncParam_Table[i][1] <= (MAX_optFuncParametersTable - 2))
			{ 
				if ((wIdParam - 2) < MAX_optFuncParametersTable)
					return DataType((WORD)optFuncParam_Table[i][wIdParam + 1], 0);
				return DataType::Null;
			}

			if (wIdParam <= optFuncParam_Table[i][1])
			{
				int j = MAX_optFuncParametersTable - 1;
				for (; optFuncParam_Table[i][j] == 0; j--); //cerca l'ultimo tipo compilato

				return DataType((WORD)optFuncParam_Table[i][j], 0);
			}
		}

	return DataType::Null;
}

//=============================================================================
//	class CStopTokens
//=============================================================================
void CStopTokens::Assign(const CStopTokens& a)
{
	m_bSkipInnerBlock			= a.m_bSkipInnerBlock;
	m_bSkipInnerRoundBrackets	= a.m_bSkipInnerRoundBrackets;
	m_bSkipInnerSquareBrackets	= a.m_bSkipInnerSquareBrackets;
	m_bSkipInnerBraceBrackets	= a.m_bSkipInnerBraceBrackets;

	m_nCountInnerBlock			= a.m_nCountInnerBlock;
	m_nCountInnerRoundBrackets	= a.m_nCountInnerRoundBrackets;
	m_nCountInnerSquareBrackets	= a.m_nCountInnerSquareBrackets;
	m_nCountInnerBraceBrackets	= a.m_nCountInnerBraceBrackets;

	RemoveAll();
	for (int i = 0; i < a.GetSize(); i++)
		Add(a.GetAt(i));
}

//-----------------------------------------------------------------------------
BOOL CStopTokens::IsStopParse (Token tk)
{
	if (m_bSkipInnerBlock)
	{
		if (tk == T_BEGIN)
			m_nCountInnerBlock++;
		else if (tk == T_END)
			m_nCountInnerBlock--;

		if (m_nCountInnerBlock > 0)
			return FALSE;
	}
	if (m_bSkipInnerRoundBrackets)
	{
		if (tk == T_ROUNDOPEN)
			m_nCountInnerRoundBrackets++;
		else if (tk == T_ROUNDCLOSE)
			m_nCountInnerRoundBrackets--;

		if (m_nCountInnerRoundBrackets > 0)
			return FALSE;
	}
	if (m_bSkipInnerSquareBrackets)
	{
		if (tk == T_SQUAREOPEN)
			m_nCountInnerSquareBrackets++;
		else if (tk == T_SQUARECLOSE)
			m_nCountInnerSquareBrackets--;

		if (m_nCountInnerSquareBrackets > 0)
			return FALSE;
	}
	if (m_bSkipInnerBraceBrackets)
	{
		if (tk == T_BRACEOPEN)
			m_nCountInnerBraceBrackets++;
		else if (tk == T_BRACECLOSE)
			m_nCountInnerBraceBrackets--;

		if (m_nCountInnerBraceBrackets > 0)
			return FALSE;
	}

	int	num = GetSize();
	for (int i = 0; i < num; i++)
		if (tk == (Token)(GetAt(i)))
			return TRUE;
	return FALSE;
}

//-----------------------------------------------------------------------------

//=============================================================================
//	class ExpItem
//=============================================================================
IMPLEMENT_DYNAMIC(ExpItem, CObject)

#ifdef _DEBUG
void ExpItem::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP1(dc, "\n\tExpItm = ", this->GetRuntimeClass()->m_lpszClassName);
}

void ExpItem::AssertValid() const
{
	__super::AssertValid();
}
#endif //_DEBUG


//=============================================================================
//	class ExpItemOpe
//=============================================================================
IMPLEMENT_DYNAMIC(ExpItemOpe, ExpItem)

//-----------------------------------------------------------------------------
ExpItemOpe::ExpItemOpe(int op, int p, const DataType& dType)
	:
	ExpItem		(p),
	m_nOpe		(op),
	m_ResultType(dType)
{
}

//-----------------------------------------------------------------------------
ExpItem* ExpItemOpe::Clone()
{
	ExpItemOpe* pItem = new ExpItemOpe(m_nOpe, m_nPosInStr, m_ResultType);

	ExpParse::DupStack(m_frstOpStack, pItem->m_frstOpStack);
	ExpParse::DupStack(m_scndOpStack, pItem->m_scndOpStack);

	return pItem;
}

//-----------------------------------------------------------------------------
ExpItem* ExpItemOpe::Expand()
{
	ExpItemOpe* pItem = new ExpItemOpe(m_nOpe, m_nPosInStr, m_ResultType);

	ExpParse::ExpandStack(m_frstOpStack, pItem->m_frstOpStack);
	ExpParse::ExpandStack(m_scndOpStack, pItem->m_scndOpStack);

	return pItem;
}

//-----------------------------------------------------------------------------
ExpItemType ExpItemOpe::IsA() const
{
	return EXP_ITEM_OPE_CLASS;
}

//-----------------------------------------------------------------------------
BOOL ExpItemOpe::IsEqual(const ExpItem& item)
{
	if (item.IsA() != EXP_ITEM_OPE_CLASS)
		return FALSE;

	const ExpItemOpe& itemOpe = (const ExpItemOpe&) item;

	return	m_nOpe == itemOpe.m_nOpe &&
			ExpParse::CompareStack(m_frstOpStack, itemOpe.m_frstOpStack) &&
			ExpParse::CompareStack(m_scndOpStack, itemOpe.m_scndOpStack);
}

//-----------------------------------------------------------------------------
OperatorType ExpItemOpe::GetType() const
{
	if	(
			m_nOpe == TT_IS_NULL		||
			m_nOpe == TT_IS_NOT_NULL	||
			m_nOpe == TT_UNMINUS		||
			m_nOpe == T_NOT				||
			m_nOpe == T_OP_NOT			||
			m_nOpe == T_BW_NOT			
		)
		return UNARY_OPR;

	if	(
			m_nOpe == T_OR				||
			m_nOpe == T_OP_OR			||
			m_nOpe == T_AND				||
			m_nOpe == T_OP_AND			||
			m_nOpe == T_BETWEEN			||
			m_nOpe == T_QUESTION_MARK
		)
		return LOGICAL_OPR;

	if (m_nOpe == TT_ESCAPED_LIKE)
		return TERNARY_OPR;
		
	return BINARY_OPR;
}

//=============================================================================
//	class ExpItemFun
//=============================================================================
IMPLEMENT_DYNAMIC(ExpItemFun, ExpItem)

//-----------------------------------------------------------------------------
ExpItemFun::ExpItemFun(Token tf, int nrParam, int p)
	:
	ExpItem		(p),
	m_nFun		(tf),
	m_nNumParam	(nrParam)
{
}

//-----------------------------------------------------------------------------
ExpItem* ExpItemFun::Clone()
{
	return new ExpItemFun(m_nFun, m_nNumParam, m_nPosInStr);
}

//-----------------------------------------------------------------------------
ExpItemType ExpItemFun::IsA() const
{
	return EXP_ITEM_FUN_CLASS;
}

//-----------------------------------------------------------------------------
BOOL ExpItemFun::IsEqual(const ExpItem& item)
{
	return	item.IsA() == EXP_ITEM_FUN_CLASS		&&
			m_nFun == ((ExpItemFun&)item).m_nFun	&&
			m_nNumParam == ((ExpItemFun&)item).m_nNumParam;
}

//=============================================================================
//	class ExpItemExternalFun
//=============================================================================
IMPLEMENT_DYNAMIC(ExpItemExternalFun, ExpItem)

//-----------------------------------------------------------------------------
ExpItemExternalFun::ExpItemExternalFun(CFunctionDescription* pFunctionDescription, int p)
	:
	ExpItem					(p),
	m_pFunctionDescription	(pFunctionDescription),
	m_nActualParameters		(0),
	m_bLateBinding			(FALSE),
	m_bIsProcedure			(FALSE)
{
}

//-----------------------------------------------------------------------------
ExpItemExternalFun::~ExpItemExternalFun()
{
	SAFE_DELETE (m_pFunctionDescription);
}

//-----------------------------------------------------------------------------
ExpItem* ExpItemExternalFun::Clone()
{
	ExpItemExternalFun* pFun = new ExpItemExternalFun(new CFunctionDescription(*m_pFunctionDescription), m_nPosInStr);
		pFun->m_nActualParameters = m_nActualParameters;
		pFun->m_bLateBinding = m_bLateBinding;
		pFun->m_bIsProcedure = m_bIsProcedure;
		pFun->m_sLateBindingName = m_sLateBindingName;

	return pFun;
}

//-----------------------------------------------------------------------------
ExpItemType ExpItemExternalFun::IsA() const
{
	return EXP_ITEM_EXTERNAL_FUN_CLASS;
}

//-----------------------------------------------------------------------------
BOOL ExpItemExternalFun::IsEqual(const ExpItem& item)
{
	if (item.IsA() != EXP_ITEM_EXTERNAL_FUN_CLASS)
		return FALSE;

	ExpItemExternalFun* pFun = (ExpItemExternalFun*) &item;
	ASSERT_KINDOF(ExpItemExternalFun, pFun);

	return	
			m_pFunctionDescription != NULL				&&
			m_pFunctionDescription->IsEqual(*(pFun->m_pFunctionDescription)) &&
			m_bLateBinding == pFun->m_bLateBinding &&
			m_bIsProcedure == pFun->m_bIsProcedure	&&
			m_sLateBindingName == pFun->m_sLateBindingName ;
}

//=============================================================================
//	class ExpItemVrb
//=============================================================================
IMPLEMENT_DYNAMIC(ExpItemVrb, ExpItem)

//-----------------------------------------------------------------------------
ExpItemVrb::ExpItemVrb(LPCTSTR pszName, int p/*=-1*/, DataObj* pData /*=NULL*/)
	:
	ExpItem		(p),
	m_strNameVrb(pszName),
	m_pData		(pData)
{
}

//-----------------------------------------------------------------------------
ExpItem* ExpItemVrb::Clone()
{
	return new ExpItemVrb(m_strNameVrb, m_nPosInStr, m_pData);
}

//-----------------------------------------------------------------------------
ExpItemType ExpItemVrb::IsA() const
{
	return EXP_ITEM_VRB_CLASS;
}

//-----------------------------------------------------------------------------
BOOL ExpItemVrb::IsEqual(const ExpItem& item)
{
	return	item.IsA() == EXP_ITEM_VRB_CLASS	&&
			m_strNameVrb.CompareNoCase(((ExpItemVrb&)item).m_strNameVrb) == 0;
}

//=============================================================================
//	class ExpItemVal
//=============================================================================
IMPLEMENT_DYNAMIC(ExpItemVal, ExpItem)

//-----------------------------------------------------------------------------
ExpItemVal::ExpItemVal(DataObj* pData, int nPos, BOOL bToBeDeleted, BOOL bOwnsData, BOOL bVoid /*= FALSE*/, BOOL bVariant /*= FALSE*/)
	:
	ExpItem			(nPos),
	m_pVal			(pData),
	m_bToBeDeleted	(bToBeDeleted),
	m_bOwnsData		(bOwnsData),
	m_bVoid			(bVoid),
	m_bVariant		(bVariant)
{
	ASSERT(pData || bVoid || bVariant);
}

//-----------------------------------------------------------------------------
ExpItemVal::~ExpItemVal()
{
	if (m_bOwnsData) SAFE_DELETE(m_pVal);
}

//-----------------------------------------------------------------------------
ExpItem* ExpItemVal::Clone	()
{
	return new ExpItemVal
			(
				m_bOwnsData && m_pVal ? m_pVal->DataObjClone() : m_pVal,
				m_nPosInStr,
				m_bToBeDeleted,
				m_bOwnsData,
				m_bVoid,
				m_bVariant
			);
}

//-----------------------------------------------------------------------------
ExpItemType	ExpItemVal::IsA	() const
{
	return EXP_ITEM_VAL_CLASS;
}

//-----------------------------------------------------------------------------
BOOL ExpItemVal::IsEqual(const ExpItem& item)
{
	return	item.IsA() == EXP_ITEM_VAL_CLASS	&& 
			m_pVal != NULL						&&
			((ExpItemVal&)item).m_pVal != NULL	&&
			m_pVal->IsEqual(*((ExpItemVal&) item).m_pVal);
}

//-----------------------------------------------------------------------------
DataType ExpItemVal::GetDataType () const
{
	if (m_bVoid)
		return DataType::Void;
	if (m_bVariant)
		return DataType::Variant;

	if (m_pVal == NULL)
	{
		ASSERT(FALSE);
		return DataType::Null;
	}
	return m_pVal->GetDataType();
}

//=============================================================================
//	class ExpItemValFromVar
//=============================================================================
IMPLEMENT_DYNAMIC(ExpItemValFromVar, ExpItemVal)
//-----------------------------------------------------------------------------
ExpItem* ExpItemValFromVar::Clone	()
{
	return new ExpItemValFromVar
			(
				m_strNameVrb,
				m_bOwnsData ? m_pVal->DataObjClone() : m_pVal,
				m_nPosInStr,
				m_bToBeDeleted,
				m_bOwnsData
			);
}

//=============================================================================
//	class ExpItemContentOfVal
//=============================================================================
IMPLEMENT_DYNAMIC(ExpItemContentOfVal, ExpItemVal)
//-----------------------------------------------------------------------------
ExpItem* ExpItemContentOfVal::Clone	()
{
	return new ExpItemContentOfVal
			(
				m_bOwnsData ? m_pVal->DataObjClone() : m_pVal,
				m_nPosInStr,
				m_bToBeDeleted,
				m_bOwnsData
			);
}

//=============================================================================
//	class ExpItemContentOfParamVal
//=============================================================================
IMPLEMENT_DYNAMIC(ExpItemContentOfParamVal, ExpItemVal)
//-----------------------------------------------------------------------------
ExpItem* ExpItemContentOfParamVal::Clone	()
{
	return new ExpItemContentOfParamVal
			(
				m_bOwnsData ? m_pVal->DataObjClone() : m_pVal,
				m_nPosInStr,
				m_bToBeDeleted,
				m_bOwnsData
			);
}

//=============================================================================
//	class ExpItemContentOfFun
//=============================================================================
IMPLEMENT_DYNAMIC(ExpItemContentOfFun, ExpItemFun)

//-----------------------------------------------------------------------------
ExpItemContentOfFun::ExpItemContentOfFun(SymTable* pSymTable, Stack*	pExpr, int p /*= -1*/)
	:
	ExpItemFun	(T_FCONTENTOF, 1, p),
	m_pSymTable (pSymTable),
	m_pExpr		(pExpr)
{
}

//-----------------------------------------------------------------------------
ExpItemContentOfFun::~ExpItemContentOfFun() 
{
	SAFE_DELETE(m_pExpr);
}

//-----------------------------------------------------------------------------
ExpItem* ExpItemContentOfFun::Expand()
{
	Expression e (m_pSymTable);
    ExpItemVal* pV = e.EvalOK(*m_pExpr);

	ExpItemContentOfVal* ret = new ExpItemContentOfVal
			(
				pV->m_bOwnsData ? pV->m_pVal->DataObjClone() : pV->m_pVal,
				pV->m_nPosInStr,
				pV->m_bToBeDeleted,
				pV->m_bOwnsData
			);

	if (pV->m_bToBeDeleted)
		delete pV;

	return ret;
}

//-----------------------------------------------------------------------------
ExpItem* ExpItemContentOfFun::Clone()
{
	Stack* pExpr = new Stack();
	ExpParse::DupStack(*m_pExpr, *pExpr);

	ExpItemContentOfFun* f = new ExpItemContentOfFun(m_pSymTable, pExpr, m_nPosInStr);

	return f;
}

//-----------------------------------------------------------------------------
BOOL ExpItemContentOfFun::IsEqual(const ExpItem& item)
{
	ExpItemContentOfFun* it = (ExpItemContentOfFun*) &item;
	ASSERT(it->IsKindOf(RUNTIME_CLASS(ExpItemContentOfFun)));

	return	__super::IsEqual(item) &&
			ExpParse::CompareStack(*m_pExpr, *(it->m_pExpr));
}

//=============================================================================
//			EXPPARSE Class
//=============================================================================
//-----------------------------------------------------------------------------
ExpParse::ExpParse(SymTable* pSymTable, CStopTokens* pStopTokens)
	:
	m_pStopTokens	(pStopTokens),
	m_pSymTable		(pSymTable),
	m_bHasExternalFunctionCall (FALSE),
	m_bHasRuleFields (FALSE),
	m_bHasDynamicFragment (FALSE)
{}

//-----------------------------------------------------------------------------
BOOL ExpParse::CompareStack(const Stack& aStack, const Stack& bStack)
{
	if (aStack.GetSize() != bStack.GetSize())	
		return FALSE;

	int numItems = aStack.GetSize();
	for (int i = 0; i < numItems; i++)
		if (!((ExpItem*) aStack[i])->IsEqual(*((ExpItem*) bStack[i])))
        	return FALSE;

	return TRUE;
}

//-----------------------------------------------------------------------------
void ExpParse::DupStack(const Stack& fromStack, Stack& toStack)
{
	toStack.ClearStack();
	int n = fromStack.GetSize();
	for (int i = n-1; i >= 0; i--)
	{
		ExpItem* p = (ExpItem*) fromStack.GetAt(i);
		toStack.Push(p->Clone());
	}
}

//-----------------------------------------------------------------------------
void ExpParse::ExpandStack(const Stack& fromStack, Stack& toStack)
{
	toStack.ClearStack();
	int n = fromStack.GetSize();
	for (int i = n-1; i >= 0; i--)
	{
		ExpItem* p = (ExpItem*) fromStack.GetAt(i);
		toStack.Push(p->Expand());
	}
}

//-----------------------------------------------------------------------------
void ExpParse::MoveStack(Stack& fromStack, Stack& toStack, int nItems /* = -1 */)
{
	int n = nItems < 0 ? fromStack.GetSize() : nItems;
	for (int i = n-1; i >= 0; i--)
	{
		toStack.Push(fromStack.GetAt(i));
		if (nItems > 0)
			fromStack.RemoveAt(i);
	}

	if (nItems < 0)
		fromStack.RemoveAll();
}

BOOL ExpParse::StopParse(Parser& lex)
{
	Token	tk	= lex.LookAhead();

	if (lex.ErrorFound())		return TRUE;
	if (tk == T_EOF)			return TRUE;
	if (tk == T_SEP)			return TRUE;
	if (m_pStopTokens == NULL)	return FALSE;

	return m_pStopTokens->IsStopParse(tk);
}

//-----------------------------------------------------------------------------
BOOL ExpParse::Parse(Parser& lex, Stack& exprStack, BOOL bKeepString)
{
	if (bKeepString)
		lex.EnableAuditString();
	
	if (!StopParse(lex) && !lex.ErrorFound())
		Expression(lex, exprStack);

	if (bKeepString)
	{
		lex.EnableAuditString(FALSE);
		m_strStringParsed = lex.GetAuditString();
	}

	if (!lex.ErrorFound() && !StopParse(lex))
		lex.SetError(_TB("Syntax error"));
		
	return !lex.ErrorFound();
}

//-----------------------------------------------------------------------------
CString ExpParse::GetString()
{
	return m_strStringParsed;
}

//-----------------------------------------------------------------------------
void ExpParse::Expression(Parser& lex, Stack& exprStack)
{
	Disjunctive(lex, exprStack);

	while ( !StopParse(lex) && !lex.ErrorFound() )
	{
		switch (lex.LookAhead())
		{
			case T_OR :
			case T_OP_OR :
			{
				ExpItemOpe* pItem = new ExpItemOpe(T_OR, lex.GetCurrentPos());
				lex.SkipToken();

				int nCurrSize = exprStack.GetSize();

				Disjunctive(lex, exprStack);

				exprStack.SetOwns(FALSE);
				MoveStack(exprStack, pItem->m_scndOpStack, exprStack.GetSize() - nCurrSize);
				exprStack.SetOwns(TRUE);

				exprStack.Push(pItem);
				break;
	        }

			case T_QUESTION_MARK :	// sintassi <x> ? <y> : <z>
			{
				ExpItemOpe* pItem = new ExpItemOpe(T_QUESTION_MARK, lex.GetCurrentPos());
				lex.SkipToken();

				int nCurrSize = exprStack.GetSize();

				Expression(lex, exprStack);
				
				exprStack.SetOwns(FALSE);
				MoveStack(exprStack, pItem->m_frstOpStack, exprStack.GetSize() - nCurrSize);
				exprStack.SetOwns(TRUE);

				if (!lex.ParseTag(T_COLON))
				{
					delete pItem;
					return;
				}

				Expression(lex, exprStack);

				exprStack.SetOwns(FALSE);
				MoveStack(exprStack, pItem->m_scndOpStack, exprStack.GetSize() - nCurrSize);
				exprStack.SetOwns(TRUE);

				exprStack.Push(pItem);
				return;
			}

			default: return;
		} // switch
	}
}

//-----------------------------------------------------------------------------
void ExpParse::Disjunctive(Parser& lex, Stack& exprStack)
{
	Conjunctive(lex, exprStack);

	while (!StopParse(lex) && !lex.ErrorFound())
	{
		switch(lex.LookAhead())
		{
			case T_AND :
			case T_OP_AND :
			{
				ExpItemOpe* pItem = new ExpItemOpe(T_AND, lex.GetCurrentPos());
				lex.SkipToken();

				int nCurrSize = exprStack.GetSize();

				Conjunctive(lex, exprStack);

				exprStack.SetOwns(FALSE);
				MoveStack(exprStack, pItem->m_scndOpStack, exprStack.GetSize() - nCurrSize);
				exprStack.SetOwns(TRUE);

				exprStack.Push(pItem);
				break;
			}
	
			default: return;
		} // switch
	}
}

//-----------------------------------------------------------------------------
void ExpParse::Conjunctive(Parser& lex, Stack& exprStack)
{
	Formula(lex, exprStack);

	while (!StopParse(lex) && !lex.ErrorFound())
	{
		int nPosNot = lex.Matched(T_NOT) ? lex.GetCurrentPos() : -1;

		switch(lex.LookAhead())
		{
			case T_GT :
			case T_LT :
			case T_LE :
			case T_GE :
			case T_NE :	case T_DIFF :
			case T_EQ :	case T_ASSIGN :
			case T_CONTAINS: case T_IN:
			{
				ExpItemOpe* pItem = new ExpItemOpe(lex.LookAhead(), lex.GetCurrentPos());
				lex.SkipToken();

				Formula(lex, exprStack);
				exprStack.Push(pItem);

				if (nPosNot >= 0)
				{
					// sintassi <x> [NOT] CONTAINS <y>
					// sintassi <x> [NOT] IN <y>	
					if (pItem->m_nOpe == T_CONTAINS || pItem->m_nOpe == T_IN)		
						exprStack.Push(new ExpItemOpe(T_NOT, nPosNot));
					else
						lex.SetError(Expression::FormatMessage (Expression::SYNTAX_ERROR));
				}
				break;
			}
			case T_BETWEEN :	// sintassi <x> [NOT] BETWEEN <y> AND <z>
			{
				ExpItemOpe* pItem = new ExpItemOpe(T_BETWEEN, lex.GetCurrentPos());
				lex.SkipToken();

				int nCurrSize = exprStack.GetSize();

				Formula(lex, exprStack);
				
				exprStack.SetOwns(FALSE);
				MoveStack(exprStack, pItem->m_frstOpStack, exprStack.GetSize() - nCurrSize);
				exprStack.SetOwns(TRUE);
				
				if (!lex.ParseTag(T_AND))
				{
					delete pItem;
					return;
				}

				Formula(lex, exprStack);

				exprStack.SetOwns(FALSE);
				MoveStack(exprStack, pItem->m_scndOpStack, exprStack.GetSize() - nCurrSize);
				exprStack.SetOwns(TRUE);

				exprStack.Push(pItem);

				if (nPosNot >= 0)
					exprStack.Push(new ExpItemOpe(T_NOT, nPosNot));

				break;
			}
			case T_LIKE :		// sintassi <x> [NOT] LIKE <y> [ ESCAPE <z> ]
			{
				ExpItemOpe* pItem = new ExpItemOpe(T_LIKE, lex.GetCurrentPos());
				lex.SkipToken();
				Formula(lex, exprStack);
				
				if (lex.Matched(T_ESCAPE))
				{
					// accettata solo costante stringa contenente un solo carattere
					CString	aString;
					if (!lex.ParseString(aString) || aString.GetLength() != 1)
					{
						lex.SetError(Expression::FormatMessage (Expression::SYNTAX_ERROR));
						delete pItem;
						return;
					}

					exprStack.Push(new ExpItemVal(new DataStr(aString), lex.GetCurrentPos()));
					
					// modifica il tipo di operatore
					pItem->m_nOpe = TT_ESCAPED_LIKE;
				}
				
				exprStack.Push(pItem);

				if (nPosNot >= 0)
					exprStack.Push(new ExpItemOpe(T_NOT, nPosNot));

				break;
			}
			case T_IS :
			{
				lex.SkipToken();
				PrivateTokens nPT = TT_IS_NULL;

				if (lex.Matched(T_NOT))
					nPT = TT_IS_NOT_NULL;

				if (!lex.ParseTag(T_NULL)) return;

				exprStack.Push(new ExpItemOpe(nPT, lex.GetCurrentPos()));
				break;
			}

			default: return;
		} // switch
	}
}

//-----------------------------------------------------------------------------
void ExpParse::Formula(Parser& lex, Stack& exprStack)
{
	Term(lex, exprStack);

	while ( !StopParse(lex) && !lex.ErrorFound() )
	{
		switch(lex.LookAhead())
		{
			case T_PLUS :
			case T_MINUS :
			{
				ExpItemOpe* pItem = new ExpItemOpe(lex.LookAhead(), lex.GetCurrentPos());
				lex.SkipToken();
				Term(lex, exprStack);
				exprStack.Push(pItem);
				break;
	        }
			default: return;
		} // switch
	}
}

//-----------------------------------------------------------------------------
void ExpParse::Term(Parser& lex, Stack& exprStack)
{
	Factor(lex, exprStack);

	while ( !StopParse(lex) && !lex.ErrorFound() )
	{
		switch(lex.LookAhead())
		{
			case T_STAR  :
			case T_SLASH :
			case T_PERC  :
			case T_EXP   :
			case T_BW_AND       : case T_BW_XOR  : case T_BW_OR :
			{
				ExpItemOpe* pItem = new ExpItemOpe(lex.LookAhead(), lex.GetCurrentPos());
                lex.SkipToken();
				Factor(lex, exprStack);
				exprStack.Push(pItem);
				break;
            }

			default: return;
		} // switch
	}
}

//-----------------------------------------------------------------------------
void ExpParse::Factor(Parser& lex, Stack& exprStack)
{
	if (lex.ErrorFound()) 
		return;

	switch(lex.LookAhead())
	{
		case T_MINUS : // unary minus
	    {
			ExpItemOpe* pUnm = new ExpItemOpe(TT_UNMINUS, lex.GetCurrentPos());
	
			lex.ParseTag(T_MINUS);
			Factor(lex, exprStack);
			exprStack.Push(pUnm);
			break;
	    }
	
		case T_BW_NOT :
		case T_NOT :
	    {
			ExpItemOpe* pNot = new ExpItemOpe(lex.LookAhead(), lex.GetCurrentPos());
	
			lex.SkipToken();
			Factor(lex, exprStack);
			exprStack.Push(pNot);
			break;
	    }
	
		case T_BRACEOPEN :
		{
			DataObj* pData = lex.ParseComplexData();
			if (!pData)
				return;
			exprStack.Push(new ExpItemVal(pData, lex.GetCurrentPos()));
			break;
		}
		case T_SQUAREOPEN :
		{
			if (this->m_bWClause)
			{
				CString strName;
				if (!lex.ParseSquaredCoupleIdent(strName))
					return;

				exprStack.Push(new ExpItemVrb(strName, lex.GetCurrentPos()));
			}
			else if (!ParseArrayCreate(lex, exprStack))	
				return;
			break;
		}
		
		case T_ROUNDOPEN :
		{
			lex.ParseTag(T_ROUNDOPEN);
			Expression(lex, exprStack);
			if (!lex.ParseTag(T_ROUNDCLOSE))	
				return;
			break;
		}
	
		case T_INT:
	    {
			int	dummy_int;
			if (!lex.ParseInt(dummy_int)) 
				return;
			exprStack.Push(new ExpItemVal( new DataInt(dummy_int), lex.GetCurrentPos()));
			break;
	    }
	
		case T_WORD:
		case T_LONG:
	    {
			long dummy_long;
	
			if (!lex.ParseLong(dummy_long)) 
				return;
			exprStack.Push(new ExpItemVal( new DataLng(dummy_long), lex.GetCurrentPos()));
			break;
	    }
	
		case T_DWORD:
		case T_DOUBLE:
	    {
			double	dummy_dbl;
	
			if (!lex.ParseDouble(dummy_dbl)) 
				return;
			exprStack.Push(new ExpItemVal(new DataDbl(dummy_dbl), lex.GetCurrentPos()));
			break;
		}
	
		case T_ID:
	    {
			CString strName;
			if (!lex.ParseID(strName)) 
				return;
			
			if (lex.LookAhead(T_ROUNDOPEN)) //funzione esterna, array/dbt methods ...
			{
				if (m_pSymTable)
				{
					int idx = strName.ReverseFind('.');
					if (idx > 0)
					{
						CString sName = strName.Left(idx);
						SymField* pField = m_pSymTable->GetField(sName);
						if (pField)
						{
							if (pField->GetProvider())
							{
								//funzione interna su array espressa in notazione object oriented
								if (!ParseArrayMethods(sName, strName.Mid(idx + 1), lex, exprStack, _T("Column_"))) 
								{
									return;
								}
								pField->IncRefCount();
								m_pSymTable->TraceFieldsUsed(pField->GetName());
								break;
							}
							else if (pField->GetDataType() == DataType::Array)
							{
								//funzione interna su array espressa in notazione object oriented
								if (!ParseArrayMethods(sName, strName.Mid(idx + 1), lex, exprStack, _T("Array_"))) 
								{
									return;
								}
								pField->IncRefCount();
								m_pSymTable->TraceFieldsUsed(pField->GetName());
								break;
							}
						}	
					}
				}

				if (!ParseExternalFunc(strName, lex, exprStack)) 
				{
					return;
				}
			}
			else if (lex.LookAhead(T_SQUAREOPEN)) //DataArray ?
			{
				if (this->m_bWClause && strName[strName.GetLength() - 1] == '.')
				{	//syntax:  table.[column]
					CString sCol;
					if (!lex.ParseSquaredIdent(sCol))
						return;
					strName.Append(sCol);

					exprStack.Push(new ExpItemVrb(strName, lex.GetCurrentPos()));
					break;
				}

				if (!ParseArrayIndexer(strName, lex, exprStack)) 
				{
					return;
				}
				SymField* pField = m_pSymTable ? m_pSymTable->GetField(strName) : NULL;
				if (pField) 
				{
					pField->IncRefCount();
					m_pSymTable->TraceFieldsUsed(pField->GetName());
				}
			}
			else // variable identifier
			{
				if (m_pSymTable)
				{
					SymField* pField = m_pSymTable->GetField(strName);
					if (pField)
					{
						pField->IncRefCount();
						m_pSymTable->TraceFieldsUsed(pField->GetName());

						this->m_bHasFields = TRUE;
						if (pField->IsRuleField())
						{
							this->m_bHasRuleFields = TRUE;
						}
						else if (pField->IsAsk())
						{
							this->m_bHasInputFields = TRUE;
							this->m_bHasAskFields = TRUE;
						}
						else if (pField->IsInput())
						{
							this->m_bHasInputFields = TRUE;
						}
					}
					else 
					{
						int idx = strName.ReverseFind('.');
						if (idx > 0)	//myRec.f_MasterCode -> Record_GetField(myRec, "f_MasterCode")
						{
							CString sRecName = strName.Left(idx);
							pField = m_pSymTable->GetField(sRecName);

							if (pField && pField->GetDataType() == DataType::SqlRecord)
							{
								exprStack.Push(new ExpItemVrb(sRecName, lex.GetCurrentPos())); //record name
								exprStack.Push(new ExpItemVal(new DataStr(strName.Mid(idx + 1)), lex.GetCurrentPos())); //field name
								exprStack.Push(new ExpItemFun(T_FSQLRECORD_GETFIELD, 2, lex.GetCurrentPos()));
								break;
							}
							else if (pField && pField->GetDataType() == DataType::Object)
							{
								CFunctionDescription* pFD = pField->FindMethod(L"GetDataObjFromColumnName");
								if (pFD)
								{
									exprStack.Push(new ExpItemVrb(sRecName, lex.GetCurrentPos()));
									exprStack.Push(new ExpItemVal(new DataStr(strName.Mid(idx + 1)), lex.GetCurrentPos())); //field name
									
									ExpItemExternalFun* pFun = new ExpItemExternalFun(pFD, lex.GetCurrentPos());
									pFun->m_nActualParameters += 2;
									exprStack.Push(pFun);
								}
								else
								{
									//TODO
									ASSERT(FALSE);
									exprStack.Push(new ExpItemVrb(sRecName, lex.GetCurrentPos())); //record name
									exprStack.Push(new ExpItemVal(new DataStr(strName.Mid(idx + 1)), lex.GetCurrentPos())); //field name

									exprStack.Push(new ExpItemFun(T_FOBJECT_GETFIELD, 2, lex.GetCurrentPos()));
								}
								break;
							}
						}
					}
				}

				exprStack.Push(new ExpItemVrb(strName, lex.GetCurrentPos()));
			}
			break;
	    }
	
		case T_STR:
	    {
			CString	aString;
	
			if (!lex.ParseString(aString)) return;
			exprStack.Push(new ExpItemVal(new DataStr (aString), lex.GetCurrentPos()));
			break;
	    }
	
		case T_TRUE:
			exprStack.Push(new ExpItemVal(new DataBool(TRUE), lex.GetCurrentPos()));
			lex.SkipToken();
			break;
	
		case T_FALSE:
			exprStack.Push(new ExpItemVal(new DataBool(FALSE), lex.GetCurrentPos()));
			lex.SkipToken();
			break;
	
		case T_FABS:		case T_FASC:		case T_FCDOW:		case T_FCEIL:
		case T_FCHR:		case T_FCMONTH:		case T_FCTOD:		case T_FDATE:
		case T_FDATETIME:	case T_FAPP_DATE:	case T_FAPP_YEAR:	case T_FDAY:
		case T_FDTOC:		case T_FFLOOR:		case T_FLEFT:		case T_FLEN:
		case T_FLOWER:		case T_FLTRIM:		case T_FMAX:		case T_FMIN:
		case T_FMOD:		case T_FMONTH:		case T_FRIGHT:
		case T_FROUND:		case T_FRTRIM:		case T_FSIGN:		case T_FSPACE:
		case T_FSTR:		case T_FSUBSTR:		case T_FSUBSTRWW:	case T_FTIME:
		case T_FTRIM:		case T_FUPPER:		case T_FVAL:		case T_FYEAR:
		case T_FINT:		case T_FLONG:		case T_FFORMAT:		
		case T_FTYPED_BARCODE: case T_FGETBARCODE_ID:
		case T_FMONTH_DAYS:	case T_FMONTH_NAME:	case T_FLAST_MONTH_DAY:	
		case T_FFIND:		case T_FREVERSEFIND:	case T_FREPLACE: case T_FREMOVENEWLINE:
		case T_FLOADTEXT:	case T_FSAVETEXT:
		case T_FELAPSED_TIME:	//@@ElapsedTime
		case T_FDAYOFWEEK:	case T_FWEEKOFMONTH: case T_FWEEKOFYEAR:	case T_FDAYOFYEAR: 
		case T_FGIULIANDATE:

		case T_FLOCALIZE:	
		case T_FISACTIVATED:
		case T_FISADMIN:

		case T_FGETAPPTITLE_FROM_NS:
		case T_FGETMODTITLE_FROM_NS:
		case T_FGETDOCTITLE_FROM_NS:

		case T_FGETCOMPANYNAME:		
		case T_FGETCOMPUTERNAME:							 
		case T_FGETCULTURE:
		case T_FGETDATABASETYPE:
		case T_FGETEDITION:
		case T_FGETINSTALLATIONNAME:							 
		case T_FGETINSTALLATIONPATH:							 
		case T_FGETINSTALLATIONVERSION:							 
		case T_FGETLOGINNAME:							 
		case T_FGETPRODUCTLANGUAGE:
		case T_FGETUSERDESCRIPTION:
		case T_FGETWINDOWUSER:	

		case T_FSETCULTURE:

		case T_FGETUPPERLIMIT:	
		case T_FMAKELOWERLIMIT:						 
		case T_FMAKEUPPERLIMIT:							 

		case T_FGETNEWGUID:
		case T_FRGB:

		case T_FTABLEEXISTS:
		case T_FGETSETTING:
		case T_FSETSETTING:
		case T_FFILEEXISTS:

		case T_FGETPATH_FROM_NS:	
		case T_FGETNS_FROM_PATH:	

		case T_FARRAY_ATTACH:
		case T_FARRAY_CLEAR:
		case T_FARRAY_COPY:
		case T_FARRAY_DETACH:
		case T_FARRAY_FIND:
		case T_FARRAY_GETAT:
		case T_FARRAY_SIZE:
		case T_FARRAY_SETAT:
		case T_FARRAY_SORT:
		case T_FARRAY_ADD:
		case T_FARRAY_APPEND:
		case T_FARRAY_INSERT:
		case T_FARRAY_REMOVE:
		case T_FARRAY_CONTAINS:
		case T_FARRAY_CREATE:
		case T_FARRAY_SUM:

		case T_FDECODE:
		case T_FCHOOSE:
		case T_FIIF:

		case T_FVALUEOF:
		case T_FISREMOTEINTERFACE:
		case T_FISRUNNINGFROMEXTERNALCONTROLLER:
		case T_FISDATABASEUNICODE:

		case T_FCONVERT:
		case T_FTYPEOF:			
		case T_FADDRESSOF:			
		case T_FEXECUTESCRIPT:
		case T_FGETTITLE:
		case T_FPREV_VALUE:
		case T_FNEXT_VALUE:

		case T_FGETTHREADCONTEXT:
		case T_FOWNTHREADCONTEXT:

		case T_FISEMPTY:
		case T_FISNULL:

		case T_FDATEADD:
		case T_FWEEKSTARTDATE:
		case T_FISLEAPYEAR:
		case T_FEASTERSUNDAY:
		case T_FWILDCARD_MATCH:

		case T_FSEND_BALLOON:
		case T_FFORMAT_TBLINK:

		case T_FCOLUMN_GETAT:
		case T_FCOLUMN_FIND:
		case T_FCOLUMN_SIZE:
		case T_FCOLUMN_SUM:

		case T_FRECORD_GETFIELD:
		case T_FSQLRECORD_GETFIELD:
		case T_FOBJECT_GETFIELD:

		case T_FISWEB:

		case T_FREPLICATE:
		case T_FPADLEFT:
		case T_FPADRIGHT:
		case T_FCOMPARE_NO_CASE:

			{
				if (!ParseInternalFunc(lex, exprStack)) 
					return;
				break;
			}

		case T_FCONTENTOF:	
			{
				if (!ParseContentOfFunc(lex, exprStack)) 
					return;
				break;
			}
	
		case T_EOF:
			lex.SetError(_TB("Unknown error"));
			return;
	
		default:
			lex.SetError(_TB("Syntax error"));
	        return;
	} // switch
}

//-----------------------------------------------------------------------------
BOOL ExpParse::ParseArrayIndexer(CString& strName, Parser& lex, Stack& exprStack)	//DataArray
{	
	if (!lex.ParseTag(T_SQUAREOPEN))	
		return FALSE;

	Token tkFun = T_FARRAY_GETAT;
	int np = 2;

	SymField* pField = NULL;
	if (m_pSymTable && (pField = m_pSymTable->GetField(strName)))
	{
		if (pField->GetProvider())
		{
			tkFun = T_FCOLUMN_GETAT;	//OK
		}
		else if (pField->GetDataType() == DataType::Array)
		{
			;	//OK
		}
		else if (pField->GetDataType() == DataType::String)
		{
			tkFun = T_FSUBSTR;
			np = 3;
		}
		else
		{
			lex.SetError(_TB("Operator indexer [] could be apply only to arrays, string or field with a data provider attached"));
			return FALSE;
		}
	}

	ExpItemFun* pFun = new ExpItemFun(tkFun, np, lex.GetCurrentPos());
	ASSERT(pFun);

	exprStack.Push(new ExpItemVrb(strName, lex.GetCurrentPos())); //array/string name

	Expression(lex, exprStack);	//array/string index

	if (tkFun == T_FSUBSTR)
		exprStack.Push(new ExpItemVal(new DataInt(1), lex.GetCurrentPos())); //substr of 1 char

	if (!lex.ParseTag(T_SQUARECLOSE))
	{
		delete pFun;
		return FALSE;
	}

	exprStack.Push(pFun);

	if (pField && tkFun == T_FARRAY_GETAT && lex.LookAhead())
	{
		CString str = lex.GetCurrentStringToken();
		if (str.GetLength() == 1 && str[0] == '.')
		{
			lex.SkipToken();

			CString sFieldName;
			if (!lex.ParseID(sFieldName)) 
				return FALSE;

#ifdef _DEBUG
			ASSERT_VALID(pField->GetData()); 
			if (pField->GetData())
			{
				DataArray* pDA = dynamic_cast<DataArray*>(pField->GetData());
				ASSERT(pDA->GetBaseDataType() == DataType::Record);
			}
#endif
			exprStack.Push(new ExpItemVal(new DataStr(sFieldName), lex.GetCurrentPos())); //field name
			exprStack.Push(new ExpItemFun(T_FSQLRECORD_GETFIELD, 2, lex.GetCurrentPos()));
		}
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL ExpParse::ParseArrayMethods(const CString& sVarName, const CString& sMethodName, Parser& lex, Stack& exprStack, LPCTSTR szPrefix)	//DataArray
{	
	if (!lex.ParseTag(T_ROUNDOPEN))	
		return FALSE;

	Token tkFun = AfxGetTokensTable()->GetKeywordsToken(szPrefix + sMethodName);
	if (tkFun == T_NOTOKEN)
		return FALSE;
	int np = parameters_of(tkFun);
	ASSERT(np != -1);

	ExpItemFun* pFun = new ExpItemFun(tkFun, np, lex.GetCurrentPos());
	ASSERT(pFun);

	exprStack.Push(new ExpItemVrb(sVarName, lex.GetCurrentPos())); //first parameter: array name

	for (int p = 2; p <= np; p++)
	{
		Expression(lex, exprStack);	//method arguments

		if (lex.ErrorFound() || (p < np && !lex.ParseTag(T_COMMA)))
		{
			delete pFun;
			return FALSE;
        }
	}

	// if the function has optional parameters then it is parsed now
	if (lex.LookAhead() != T_ROUNDCLOSE)
	{
		if (!ParseOptionalParameters(pFun, lex, exprStack, 1))
		{
			delete pFun;
			return FALSE;
		}
	}
			
	if (!lex.ParseTag(T_ROUNDCLOSE))
	{
		delete pFun;
		return FALSE;
	}

	exprStack.Push(pFun);
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL ExpParse::ParseContentOfFunc(Parser& lex, Stack& exprParentStack, BOOL bNative /*= FALSE*/)
{
	m_bHasDynamicFragment = TRUE;

	Token tokenFun = lex.LookAhead();
	
	lex.SkipToken();
	if (!lex.ParseTag(T_ROUNDOPEN))	return FALSE;
	
	int nrParam	= parameters_of(tokenFun);
	ASSERT(nrParam != -1);
			
	Stack* exprStack = new Stack();

	this->Expression (lex, *exprStack);
	if (lex.ErrorFound())
	{
		delete exprStack;
		return FALSE;
    }
			
	if (!lex.ParseTag(T_ROUNDCLOSE))
	{
		delete exprStack;
		return FALSE;
	}

	::Expression e (this->m_pSymTable);
	DataType t = e.CompileOK(lex, *exprStack);
	//if (!e.Compatible(t, DataType::String))
	//	return lex.SetError(FormatMessage(RETTYPE));

	if (bNative)
	{
		ExpItemContentOfFun* pContentOf = new ExpItemContentOfFun(m_pSymTable, exprStack, lex.GetCurrentPos());
		exprParentStack.Add(pContentOf);	//ADD
	}
	else
	{
		ExpItemContentOfParamVal* param = new ExpItemContentOfParamVal(new DataStr(_T("ContentOf parameter")));
		exprParentStack.Push(param);
	
		ExpItemContentOfFun* pContentOf = new ExpItemContentOfFun(m_pSymTable, exprStack, lex.GetCurrentPos());
		exprParentStack.Push(pContentOf);	//PUSH
	}
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL ExpParse::ParseInternalFunc(Parser& lex, Stack& exprStack)
{
	Token tokenFun = lex.SkipToken();

	if (!lex.ParseTag(T_ROUNDOPEN))	return FALSE;
	
	int nrParam	= parameters_of(tokenFun);
	ASSERT(nrParam != -1);
			
	ExpItemFun* pFun = new ExpItemFun(tokenFun, nrParam, lex.GetCurrentPos());
	ASSERT(pFun);
			
	for (int p = 1; p <= nrParam; p++)
	{
		if (p == 1 && tokenFun == T_FCONVERT)
		{
			DataType dt, basedt;
			if (!lex.ParseDataType(dt, basedt))
				return FALSE;

			DataObj* pDummyVal = DataObj::DataObjCreate(dt);
			if (dt == DataType::Array)
			{
				((DataArray*)pDummyVal)->SetBaseDataType(basedt);
			}
			exprStack.Push(new ExpItemVal(pDummyVal, lex.GetCurrentPos()));
		}
		else if (p == 1 && tokenFun == T_FADDRESSOF)
		{
			CString sIdent;
			if (!lex.ParseID(sIdent))
				return FALSE;

			SymField* pField = m_pSymTable->GetField(sIdent);
			if (!pField)
			{
				lex.SetError(Expression::FormatMessage (Expression::UNKNOWN_FIELD));
				return FALSE;
			}
			long ptr = (long) pField->GetData();
			if (!ptr)
			{
				lex.SetError(Expression::FormatMessage (Expression::EXTERNAL_FUNC_BAD_PARAMETER));				
				return FALSE;
			}

			DataLng* pointer = new DataLng(ptr);
			pointer->SetAsHandle(); pointer->SetValid(TRUE);
			
			exprStack.Push(new ExpItemVal(pointer, lex.GetCurrentPos()));
		}
		else  if (p == 1 && (tokenFun == T_FPREV_VALUE || tokenFun == T_FNEXT_VALUE))
		{
			CString sIdent;
			if (!lex.ParseID(sIdent))
				return FALSE;

			SymField* pField = m_pSymTable->GetField(sIdent);
			if (!pField)
			{
				lex.SetError(Expression::FormatMessage(Expression::UNKNOWN_FIELD));
				return FALSE;
			}
			
			exprStack.Push(new ExpItemVrb(sIdent, lex.GetCurrentPos()));
		}
		else  if (p == 1 && tokenFun == T_FGETTITLE)
		{
			CString sIdent;
			if (lex.LookAhead(T_STR))
			{
				if (!lex.ParseString(sIdent))
					return FALSE;
			}
			else if (!lex.ParseID(sIdent))
					return FALSE;

			SymField* pField = m_pSymTable->GetField(sIdent);
			if (!pField)
			{
				lex.SetError(Expression::FormatMessage(Expression::UNKNOWN_FIELD));
				return FALSE;
			}

			exprStack.Push(new ExpItemVrb(sIdent, lex.GetCurrentPos()));
		}
		else if (p == 2 && tokenFun == T_FGETTHREADCONTEXT)
		{
			CString sIdent;
			if (!lex.ParseID(sIdent))
				return FALSE;

			SymField* pField = m_pSymTable->GetField(sIdent);
			if (!pField)
			{
				lex.SetError(Expression::FormatMessage (Expression::UNKNOWN_FIELD));
				return FALSE;
			}

			//exprStack.Push(new ExpItemVrb(sIdent, lex.GetCurrentPos()));
			exprStack.Push(new ExpItemVal(new DataStr(sIdent), lex.GetCurrentPos()));
		}
		else if (p == 1 && tokenFun == T_FOWNTHREADCONTEXT)
		{
			CString sIdent;
			if (!lex.ParseID(sIdent))
				return FALSE;

			SymField* pField = m_pSymTable->GetField(sIdent);
			if (!pField)
			{
				lex.SetError(Expression::FormatMessage (Expression::UNKNOWN_FIELD));
				return FALSE;
			}

			//exprStack.Push(new ExpItemVrb(sIdent, lex.GetCurrentPos()));
			exprStack.Push(new ExpItemVal(new DataStr(sIdent), lex.GetCurrentPos()));
		}
		else
			Expression(lex, exprStack);

		if (lex.ErrorFound() || (p < nrParam && !lex.ParseTag(T_COMMA)))
		{
			delete pFun;
			return FALSE;
        }
	}
	
	// if the function has optional parameters then it is parsed now
	if (lex.LookAhead() != T_ROUNDCLOSE)
	{
		if (!ParseOptionalParameters(pFun, lex, exprStack))
		{
			delete pFun;
			return FALSE;
		}
	}
			
	if (!lex.ParseTag(T_ROUNDCLOSE))
	{
		delete pFun;
		return FALSE;
	}
			
	exprStack.Push(pFun);
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL ExpParse::ParseArrayCreate(Parser& lex, Stack& exprStack)
{
	if (!lex.Match(T_SQUAREOPEN))	
		return FALSE;
	
	int nrParam	= 1;	
	for (; ; nrParam++)
	{
		Expression(lex, exprStack);

		if (lex.ErrorFound())
		{
			return FALSE;
        }

		if (lex.Matched(T_SQUARECLOSE))
		{
			break;
		}

		if (!lex.Match(T_COMMA))
		{
			return FALSE;
		}
	}

	ExpItemFun* pFun = new ExpItemFun(T_FARRAY_CREATE, nrParam, lex.GetCurrentPos());
	ASSERT(pFun);			
	exprStack.Push(pFun);
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL ExpParse::IsVariantArgumentsFunction(CString strFuncName)
{
	int idx = strFuncName.ReverseFind('.');
	if (idx > 0)
	{
		CString sPrefix = strFuncName.Left(idx);
		if (
			sPrefix.CompareNoCase(_T("Woorm")) &&
			sPrefix.CompareNoCase(_T("RS")) &&
			sPrefix.CompareNoCase(_NS_WEB("Framework.TbWoormViewer.TbWoormViewer"))
			)
			return FALSE;

		strFuncName =  strFuncName.Mid(idx + 1);
	}

	return 
		strFuncName.CompareNoCase(_NS_WEB("RunReport")) == 0 
		||
		strFuncName.CompareNoCase(_NS_WEB("BrowseDocument")) == 0 
		||
		strFuncName.CompareNoCase(_NS_WEB("ExpandTemplate")) == 0 
		||
		strFuncName.CompareNoCase(_NS_WEB("ConnectionLockRecord")) == 0
		||
		strFuncName.CompareNoCase(_NS_WEB("ConnectionUnlockRecord")) == 0
		;
}

//-----------------------------------------------------------------------------
BOOL ExpParse::AllowLateBinding (CString strFuncName, CString& sHandleName, SymTable*)
{
	sHandleName.Empty();
	int idx = strFuncName.Find('.');
	if (idx > 0)
	{
		CString sPrefix = strFuncName.Left(idx);
		if (
			sPrefix.CompareNoCase(_NS_WRM("OwnerID")) == 0 ||
			sPrefix.CompareNoCase(_NS_WRM("LinkedDocumentID")) == 0
			)
		{
			sHandleName = sPrefix;
			return TRUE;
		}
		CString sMethod = strFuncName.Mid(idx + 1);
		if (sMethod.Find('.') < 0 && sPrefix.CompareNoCase(_T("Woorm")) && sPrefix.CompareNoCase(_T("RS")))
			return TRUE;	//? qry.Read

		return FALSE;
	}
	return TRUE;	//procedure ?
}

//-----------------------------------------------------------------------------
BOOL ExpParse::ParseExternalFunc(CString& strName, Parser& lex, Stack& exprStack)
{
	if (!lex.ParseTag(T_ROUNDOPEN))	
		return FALSE;
	
	CString strFuncName = strName;

	BOOL bAllowVariantArgs = IsVariantArgumentsFunction(strFuncName);
	BOOL bAllowLateBinding = FALSE;
	BOOL bIsQuery = FALSE;

	CFunctionDescription *pFuncPrototype = new CFunctionDescription();
	CString sHandleName;

	CTBNamespace ns1(CTBNamespace::FUNCTION, strFuncName);
	BOOL bOk = AfxGetTbCmdManager()->GetFunctionDescription(ns1, *pFuncPrototype, FALSE);
	if (!bOk && m_pSymTable)
	{
		//try to resolve object oriented syntax call by reverse: Control.Method (a,b) -> ns-method.class_Method (Control, a, b)
		bOk = m_pSymTable->ResolveCallMethod(strFuncName, *pFuncPrototype, sHandleName);
		if (!bOk)
		{
			//try to resolve shortcut syntax by expand aliases such as: Woorm.Method (a,b) -> Framework.TbWoormViever.TbWoormViever.Method (a, b)
			CString sExpanded;
			if (m_pSymTable->ExpandAlias(strFuncName, sExpanded))
			{
				CTBNamespace ns2(CTBNamespace::FUNCTION, sExpanded);
				bOk = AfxGetTbCmdManager()->GetFunctionDescription(ns2, *pFuncPrototype, FALSE);
			}
		}
		if (!bOk)
		{
			bOk = m_pSymTable->ResolveCallQuery(strFuncName, *pFuncPrototype, sHandleName);
			if (bOk)
				bIsQuery = TRUE;
		}
		if (!bOk)
		{
			bAllowLateBinding = AllowLateBinding(strFuncName, sHandleName, m_pSymTable);
			if (bAllowLateBinding)
			{
				bAllowVariantArgs = TRUE;
				pFuncPrototype->SetName(strFuncName);
				pFuncPrototype->SetReturnValueDescription (CDataObjDescription(_T(""), DataType::Variant, CDataObjDescription::_OUT));
				bOk = TRUE;
			}
		}
	}

	if (!pFuncPrototype || !bOk)
		return lex.SetError(cwsprintf(_TB("The searched function \"{0-%s} is not found in any installed module."), (LPCTSTR) strFuncName));

	ExpItemExternalFun* pFun = NULL;
	if (bOk)
	{
		pFun = new ExpItemExternalFun(pFuncPrototype, lex.GetCurrentPos());

		pFun->m_sLateBindingName = strFuncName;	//magari è stata risolta, serve per l'umparsing

		if (bAllowLateBinding)
		{
			pFun->m_bLateBinding = TRUE;
		}

		if (!sHandleName.IsEmpty() && (pFuncPrototype->GetParameters().GetSize() >= 1 || bAllowLateBinding))
		{
			//insert auto handle param such as: Document.Method(a,b) -> ns-method.classdoc_Method(DOCUMENT,  a, b)
			if (bIsQuery)	//QueryRead("query-name")
				exprStack.Push(new ExpItemVal(new DataStr(sHandleName), lex.GetCurrentPos()) );
			else
				exprStack.Push(new ExpItemVrb(sHandleName, lex.GetCurrentPos()));

			pFun->m_nActualParameters++;
		}

		if (lex.LookAhead(T_ROUNDCLOSE))
		{
			if (pFun->m_nActualParameters < pFuncPrototype->GetParameters().GetSize())
			{
				CDataObjDescription* pDO = pFuncPrototype->GetParamDescription(pFun->m_nActualParameters);

				if (!pDO->IsOptional())
				{
					lex.SetError(Expression::FormatMessage (Expression::EXTERNAL_FUNC_MISSING_PARAMETERS) + _T(" - ") +  (pFuncPrototype ? pFuncPrototype->GetName() : (LPCTSTR) strFuncName));
					SAFE_DELETE(pFun);
					return FALSE;
				}
			}
		}
		else while (!lex.LookAhead(T_ROUNDCLOSE))
		{
			BOOL bForceConst = lex.Matched(T_CONST);

			Expression(lex, exprStack);

			if (lex.ErrorFound())
			{
				lex.SetError(Expression::FormatMessage (Expression::EXTERNAL_FUNC_BAD_PARAMETER) + _T(" - ") +  (pFuncPrototype ? pFuncPrototype->GetName() : (LPCTSTR) strFuncName));
				SAFE_DELETE(pFun);
				return FALSE;
			}

			pFun->m_nActualParameters++;

			if (pFun->m_nActualParameters > pFuncPrototype->GetParameters().GetSize())
			{
				if (!bAllowVariantArgs)
				{
					lex.SetError(Expression::FormatMessage (Expression::EXTERNAL_FUNC_TOOMANY_PARAMETERS) + _T(" - ") +  (pFuncPrototype ? pFuncPrototype->GetName() : (LPCTSTR) strFuncName));
					SAFE_DELETE(pFun);
					return FALSE;
				}
			}

			if (lex.LookAhead(T_ROUNDCLOSE))
			{
				if (pFun->m_nActualParameters < pFuncPrototype->GetParameters().GetSize())
				{
					CDataObjDescription* pDO = pFuncPrototype->GetParamDescription(pFun->m_nActualParameters);

					if (pDO->IsOptional())
						break;

					lex.SetError(Expression::FormatMessage (Expression::EXTERNAL_FUNC_MISSING_PARAMETERS) + _T(" - ") +  (pFuncPrototype ? pFuncPrototype->GetName() : (LPCTSTR) strFuncName));
					SAFE_DELETE(pFun);
					return FALSE;
				}
				break;
			}

			if (!lex.Matched(T_COMMA))
			{
				lex.SetError(Expression::FormatMessage (Expression::EXTERNAL_FUNC_MISSING_COMMA) + _T(" - ") +  (pFuncPrototype ? pFuncPrototype->GetName() : (LPCTSTR) strFuncName));
				SAFE_DELETE(pFun);
				return FALSE;
			}
		}
	}
	else
		SAFE_DELETE(pFuncPrototype);

	if (pFun == NULL)
		return lex.SetError(Expression::FormatMessage (Expression::UNKNOWN_EXTERNAL_FUNC) + _T(" - ") + (pFuncPrototype ? pFuncPrototype->GetName() : (LPCTSTR) strFuncName));

	if (!lex.ParseTag(T_ROUNDCLOSE))
	{
		lex.SetError(Expression::FormatMessage (Expression::EXTERNAL_FUNC_MISSING_ROUNDCLOSE) + _T(" - ") +  (pFuncPrototype ? pFuncPrototype->GetName() : (LPCTSTR) strFuncName));
		SAFE_DELETE(pFun);
		return FALSE;
	}
	
	ASSERT_VALID(pFun);
	if (pFuncPrototype) ASSERT_VALID(pFuncPrototype);

	exprStack.Push(pFun);

	m_bHasExternalFunctionCall = TRUE;

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL ExpParse::ParseOptionalParameters
	(
		ExpItemFun*	pExpItemFun, 
		Parser&		lex, 
		Stack&		exprStack,
		int			nH /*=0*/
	)
{
	int nOptParam = opt_parameters_of(pExpItemFun->m_nFun);
	if (nOptParam == -1)
		return lex.SetError(_TB("Wrong parameter for the function"));
	
	int np = parameters_of(pExpItemFun->m_nFun) - nH;
	for (int i = 0; i < nOptParam ; i++)
	{
		if	((i != 0 || np > 0) && !lex.ParseTag(T_COMMA))
			return FALSE;

		Expression(lex, exprStack);
		if (lex.ErrorFound())
			return FALSE;
			
		pExpItemFun->m_nNumParam++;

		if (lex.LookAhead(T_ROUNDCLOSE))
			break;
	}
	
	return TRUE;
}

//=============================================================================
//			EXPUNPARSE Class
//=============================================================================
//-----------------------------------------------------------------------------
BOOL ExpUnparse::Unparse(CString& exprStr, Stack& exprStack)
{
	CStringArray outStrings;
	Stack tmpStack;

	ExpParse::DupStack(exprStack, tmpStack);

	if (!Expression(outStrings, tmpStack))	return FALSE;
	
	exprStr.Empty();
	for(int i = outStrings.GetUpperBound(); i >= 0; i--)
		exprStr += outStrings[i];
		
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL ExpUnparse::Expression(CStringArray& outStrings, Stack& workStack)
{
	CString strTmp;
	BOOL ok = TRUE;
	ExpItem* item=(ExpItem*)workStack.Top();
	if (!item)
		return FALSE;

	switch (item->IsA())
	{
		case EXP_ITEM_VRB_CLASS:
		{
			ExpItemVrb* itemVrb = (ExpItemVrb*) workStack.Pop();
			ok = UnparseVariable(strTmp, itemVrb) && ok;

			// dopo ogni Pop bisogna deletare l'elemento
			delete itemVrb;

			if (!ok) return FALSE;
			outStrings.Add(strTmp);
			break;
		}
		case EXP_ITEM_VAL_CLASS:
		{
			ExpItemVal* itemVal = (ExpItemVal*) workStack.Pop();
			ok = UnparseValue(strTmp, itemVal);

			// dopo ogni Pop bisogna deletare l'elemento
			delete itemVal;

			if (!ok) return FALSE;
			outStrings.Add(strTmp);
			break;
		}
		case EXP_ITEM_OPE_CLASS:
		{
			outStrings.Add(_T(" )"));

			ExpItemOpe*		itemOpe	= (ExpItemOpe*) workStack.Pop();
			int				nOpe	= itemOpe->m_nOpe;
			OperatorType	nOpeType= itemOpe->GetType();

			if (nOpeType == LOGICAL_OPR)
			{
				if (!Unparse(strTmp, itemOpe->m_scndOpStack))
				{
					// dopo ogni Pop bisogna deletare l'elemento
					delete itemOpe;
					return FALSE;
				}

				outStrings.Add(strTmp);

				if (nOpe == T_QUESTION_MARK || nOpe == T_BETWEEN)
				{
					if (nOpe == T_QUESTION_MARK)
						ok = UnparseOpe(strTmp, T_COLON);

					if (nOpe == T_BETWEEN)
						ok = UnparseOpe(strTmp, T_AND);

					if (!ok)
					{
						// dopo ogni Pop bisogna deletare l'elemento
						delete itemOpe;
						return FALSE;
					}

					outStrings.Add(strTmp);

					if (!Unparse(strTmp, itemOpe->m_frstOpStack))
					{
						// dopo ogni Pop bisogna deletare l'elemento
						delete itemOpe;
						return FALSE;
					}

					outStrings.Add(strTmp);
				}
			}

			// dopo ogni Pop bisogna deletare l'elemento
			delete itemOpe;
	
			if (nOpe != TT_IS_NULL && nOpe != TT_IS_NOT_NULL)
			{
				if (nOpeType != LOGICAL_OPR && !Expression(outStrings, workStack))
					return FALSE;

				if (nOpe == TT_ESCAPED_LIKE)
				{
					if (!UnparseOpe(strTmp, T_ESCAPE)) return FALSE;
	
					outStrings.Add(strTmp);
	
					if (!Expression(outStrings, workStack)) return FALSE;
				}
			}
					
			if (!UnparseOpe(strTmp, nOpe))	return FALSE;
			
			outStrings.Add(strTmp);

			if	(
					(nOpeType != UNARY_OPR || nOpe == TT_IS_NULL || nOpe == TT_IS_NOT_NULL) &&
					!Expression(outStrings, workStack)
				)
				return FALSE;

			outStrings.Add(_T(" ("));
			break;
	    }
		case EXP_ITEM_FUN_CLASS:
		{
			outStrings.Add(_T(" )"));

			ExpItemFun* itemFun = (ExpItemFun*) workStack.Pop();
			int			nParam	= itemFun->m_nNumParam;
			int			nFun	= itemFun->m_nFun;
			
			// dopo ogni Pop bisogna deletare l'elemento
			delete itemFun;
	
			for (int p = nParam; p > 0; p--)
			{
				if (!Expression(outStrings, workStack)) return FALSE;
				if (p > 1 )
					outStrings.Add(_T(","));
			}

			strTmp = AfxGetTokensTable()->ToString((Token)nFun);

			outStrings.Add(_T(" ("));
			outStrings.Add(strTmp);
			outStrings.Add(_T(" "));
			break;
		}
		case EXP_ITEM_EXTERNAL_FUN_CLASS:
		{
			outStrings.Add(_T(" )"));

			ExpItemExternalFun* itemFun = (ExpItemExternalFun*) workStack.Pop();
			int nParam	= itemFun->m_nActualParameters;

			strTmp = (itemFun->m_sLateBindingName.IsEmpty() ?
							itemFun->m_pFunctionDescription->GetNamespace().ToUnparsedString() :
							itemFun->m_sLateBindingName);

			//CString	strT_Const = _T(" ") + cwsprintf(T_CONST) + _T(" ");

			for (int p = nParam; p > 0; p--)
			{
				if (!Expression(outStrings, workStack)) 
				{
					// dopo ogni Pop bisogna deletare l'elemento
					delete itemFun;
					return FALSE;
				}
				ASSERT(itemFun->m_pFunctionDescription);

				//if (itemFun->m_pFunctionDescription->GetParamDescription(p-1)->IsPassedModeIn())
				//	outStrings.Add(strT_Const);

				if (p > 1)
					outStrings.Add(_T(","));
			}

			outStrings.Add(_T(" ("));
			outStrings.Add(strTmp);

			// dopo ogni Pop bisogna deletare l'elemento
			delete itemFun;
			break;
		}
	} // switch

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL ExpUnparse::UnparseVariable(CString& exprStr, ExpItemVrb* pExpItemVrb)
{
	exprStr = CString(" ") + pExpItemVrb->m_strNameVrb;

    return TRUE;
}

//-----------------------------------------------------------------------------
BOOL ExpUnparse::UnparseValue(CString& exprStr, ExpItemVal* pExpItemVal)
{
	if (pExpItemVal->IsKindOf(RUNTIME_CLASS(ExpItemContentOfVal)))
	{
		DataStr* ds = (DataStr*) pExpItemVal->m_pVal;
		ASSERT(ds->IsKindOf(RUNTIME_CLASS(DataStr)));

		Unparser::UnparseEscapedString(ds->GetString(), exprStr);
		return TRUE;
	}

	exprStr = CString(" ") + UnparseData(*(pExpItemVal->m_pVal));
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL ExpUnparse::UnparseOpe(CString& exprStr, int nOpe)
{
	CString str;
	
	exprStr = " ";		// sicuramente si evitano gli operatori compositi

	switch (nOpe)
    {
		case T_NOT:
		case T_OP_NOT:
			str = cwsprintf(T_NOT);
			break;

		case TT_UNMINUS:
			str = cwsprintf(T_MINUS);
			break;

		case TT_IS_NULL:
		case TT_IS_NOT_NULL:
			str = cwsprintf(T_IS);
			
			exprStr += str + _T(" ");
			if (nOpe == TT_IS_NOT_NULL)
			{
				str = cwsprintf(T_NOT);
				exprStr += str + _T(" ");
			}

			str = cwsprintf(T_NULL);
			
			break;

		case TT_ESCAPED_LIKE:
			str = cwsprintf(T_LIKE);
			break;

		case T_AND:
		case T_OP_AND:
			str = cwsprintf(T_AND);
			break;

		case T_OR:
		case T_OP_OR:
			str = cwsprintf(T_OR);
			break;

		case T_NE:
		case T_DIFF:
			str = cwsprintf(T_DIFF);
			break;

		case T_EQ:
		case T_ASSIGN:
			str = cwsprintf(T_ASSIGN);
			break;
			
		default :
			str = cwsprintf((Token)nOpe);
			break;

	}

	exprStr += str;
	
    return !str.IsEmpty();                                                        
}

//-----------------------------------------------------------------------------
CString ExpUnparse::UnparseData(const DataObj& dataObj)
{
	CString exprStr;
	
	switch (dataObj.GetDataType().m_wType)
	{
		case DATA_STR_TYPE	:
		case DATA_TXT_TYPE	:	
		{
			exprStr = "\"";
			Unparser::UnparseEscapedString(((DataStr&)dataObj).GetString(), exprStr);
			exprStr += "\"";
			break;
        }
		case DATA_ENUM_TYPE	:
		{
			exprStr = Unparser::UnparseEnumItem(((const DataEnum&) dataObj).GetValue());
			break;
		}	
		case DATA_DATE_TYPE	:
		{
			exprStr	= Unparser::UnparseDateTime((const DataDate&) dataObj);
			break;
		}
		case DATA_LNG_TYPE	:	//@@ElapsedTime
		{
			exprStr	= dataObj.IsATime()
						? Unparser::UnparseElapsedTime((const DataLng&) dataObj)
						: dataObj.Str();
			break;
		}

		default	:
			exprStr = dataObj.Str();
	}
	
	return exprStr;
}
