#ifdef DOTNET45
  #define use_dotnetfx45
#endif
#ifdef DOTNET40
  #define use_dotnetfx40
#endif
#ifdef DOTNET35
  #define use_dotnetfx35
#endif
#ifdef DOTNET2
  #define use_dotnetfx20
#endif
#ifdef NEED_MDAC
  #define use_mdac28
#endif
#ifdef NEED_JETDB
  #define use_jet4sp8
#endif
#ifdef NEED_ACE
  #define use_ace
#endif

//#define lang_german

// Define stuff we need to download/update/install

//#define use_iis
//#define use_kb835732
//#define use_kb886903
//#define use_kb928366

//#define use_msi20
//#define use_msi31
//#define use_msi45
//#define use_ie6

//#define use_dotnetfx11
// German languagepack?
//#define use_dotnetfx11lp

//#define use_dotnetfx20
// German languagepack?
//#define use_dotnetfx20lp

//#define use_dotnetfx35
// German languagepack?
//#define use_dotnetfx35lp

// dotnet_Passive enabled shows the .NET/VC2010 installation progress, as it can take quite some time
//#define dotnet_Passive
//#define use_dotnetfx40
//#define use_vc2010

//#define use_mdac28
//#define use_jet4sp8
// SQL 3.5 Compact Edition
//#define use_scceruntime
// SQL Express
//#define use_sql2005express
//#define use_sql2008express

// Enable the required define(s) below if a local event function (prepended with Local) is used
//#define haveLocalPrepareToInstall
//#define haveLocalNeedRestart
//#define haveLocalNextButtonClick

[Setup]
ShowLanguageDialog=no
LanguageDetectionMethod=none

[Languages]
Name: "en"; MessagesFile: "compiler:Default.isl"
Name: "de"; MessagesFile: "compiler:Languages\German.isl"

[CustomMessages]
winxpsp3_title=Install Windows XP Service Pack 3 first.

#include "products.iss"

#include "products\winversion.iss"
#include "products\fileversion.iss"

#ifdef use_iis
#include "products\iis.iss"
#endif

#ifdef use_kb835732
#include "scripts\products\kb835732.iss"
#endif
#ifdef use_kb886903
#include "products\kb886903.iss"
#endif
#ifdef use_kb928366
#include "products\kb928366.iss"
#endif

#ifdef use_msi20
#include "products\msi20.iss"
#endif
#ifdef use_msi31
#include "products\msi31.iss"
#endif
#ifdef use_msi45
#include "products\msi45.iss"
#endif
#ifdef use_ie6
#include "products\ie6.iss"
#endif

#ifdef use_dotnetfx11
#include "products\dotnetfx11.iss"
#include "products\dotnetfx11lp.iss"
#include "products\dotnetfx11sp1.iss"
#endif

#ifdef use_dotnetfx20
#include "products\dotnetfx20.iss"
#ifdef use_dotnetfx20lp
#include "products\dotnetfx20lp.iss"
#endif
#include "products\dotnetfx20sp1.iss"
#ifdef use_dotnetfx20lp
#include "products\dotnetfx20sp1lp.iss"
#endif
#include "products\dotnetfx20sp2.iss"
#ifdef use_dotnetfx20lp
#include "products\dotnetfx20sp2lp.iss"
#endif
#endif

#ifdef use_dotnetfx35
#include "products\dotnetfx35.iss"
#ifdef use_dotnetfx35lp
#include "products\dotnetfx35lp.iss"
#endif
#include "products\dotnetfx35sp1.iss"
#ifdef use_dotnetfx35lp
#include "products\dotnetfx35sp1lp.iss"
#endif
#endif

#ifdef use_dotnetfx40
//#include "products\dotnetfx40client.iss"
//#include "products\dotnetfx40full.iss"
#include "products\_dotnetfx40full.iss"
#endif

#ifdef use_dotnetfx45
#include "products\dotnetfx45full.iss"
#endif

#ifdef use_vc2010
#include "products\vc2010.iss"
#endif

#ifdef use_mdac28
#include "products\mdac28.iss"
#endif
#ifdef use_jet4sp8
#include "products\jet4sp8.iss"
#endif
// SQL 3.5 Compact Edition
#ifdef use_scceruntime
#include "products\scceruntime.iss"
#endif
// SQL Express
#ifdef use_sql2005express
#include "products\sql2005express.iss"
#endif
#ifdef use_sql2008express
#include "products\sql2008express.iss"
#endif

#ifdef use_ace
#include "products\_ace.iss"
#endif

[Code]
#ifdef DotNetInitializeSetup
function DotNetInitializeSetup(): Boolean;
#else
function InitializeSetup(): Boolean;
#endif
begin
	//init windows version
	initwinversion();
	
	//check if dotnetfx20 can be installed on this OS
	//if not minwinspversion(5, 0, 3) then begin
	//	MsgBox(FmtMessage(CustomMessage('depinstall_missing'), [CustomMessage('win2000sp3_title')]), mbError, MB_OK);
	//	exit;
	//end;
	if not minwinspversion(5, 1, 3) then begin
		MsgBox(FmtMessage(CustomMessage('depinstall_missing'), [CustomMessage('winxpsp3_title')]), mbError, MB_OK);
		exit;
	end;
	
#ifdef use_iis
	if (not iis()) then exit;
#endif	
	
#ifdef use_msi20
	msi20('2.0');
#endif
#ifdef use_msi31
	msi31('3.1');
#endif
#ifdef use_msi45
	msi45('4.5');
#endif
#ifdef use_ie6
	ie6('5.0.2919');
#endif
	
#ifdef use_dotnetfx11
	dotnetfx11();
#ifdef use_dotnetfx11lp
	dotnetfx11lp();
#endif
	dotnetfx11sp1();
#endif
#ifdef use_kb886903
	kb886903(); //better use windows update
#endif
#ifdef use_kb928366
	kb928366(); //better use windows update
#endif
	
	//install .netfx 2.0 sp2 if possible; if not sp1 if possible; if not .netfx 2.0
#ifdef use_dotnetfx20
	if minwinversion(5, 1) then begin
		dotnetfx20sp2();
#ifdef use_dotnetfx20lp
		dotnetfx20sp2lp();
#endif
	end else begin
		if minwinversion(5, 0) and minwinspversion(5, 0, 4) then begin
#ifdef use_kb835732
			kb835732();
#endif
			dotnetfx20sp1();
#ifdef use_dotnetfx20lp
			dotnetfx20sp1lp();
#endif
		end else begin
			dotnetfx20();
#ifdef use_dotnetfx20lp
			dotnetfx20lp();
#endif
		end;
	end;
#endif
	
#ifdef use_dotnetfx35
	dotnetfx35();
#ifdef use_dotnetfx35lp
	dotnetfx35lp();
#endif
	dotnetfx35sp1();
#ifdef use_dotnetfx35lp
	dotnetfx35sp1lp();
#endif
#endif
	
	// If no .NET 4.0 framework found, install the smallest
#ifdef use_dotnetfx40
	dotnetfx40full(false);
#endif

	// If no .NET 4.5 framework found, install the smallest
#ifdef use_dotnetfx45
	dotnetfx45full(false);
#endif

	// Visual C++ 2010 Redistributable
#ifdef use_vc2010
	vc2010();
#endif
	
#ifdef use_mdac28
	mdac28('2.7');
#endif
#ifdef use_jet4sp8
	jet4sp8('4.0.8015');
#endif
	// SQL 3.5 CE
#ifdef use_ssceruntime
	ssceruntime();
#endif
	// SQL Express
#ifdef use_sql2005express
	sql2005express();
#endif
#ifdef use_sql2008express
	sql2008express();
#endif
#ifdef use_ace 
  ace();
#endif
	
	Result := true;
end;

