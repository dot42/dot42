// http://support.microsoft.com/kb/239114

[CustomMessages]
ace_title=Office 2007 Access Connectivity Engine

en.ace_size=3.7 MB
de.ace_size=3,7 MB


[Code]
const
  ace_url = 'http://download.microsoft.com/download/f/d/8/fd8c20d8-e38a-48b6-8691-542403b91da1/AccessDatabaseEngine.exe';

procedure ace();
begin
	//check for Jet4 Service Pack 8 installation
  if (not RegKeyExists(HKLM32, 'Software\Microsoft\Office\12.0\Access Connectivity Engine')) then 
  begin
		AddProduct('AccessConnectivityEngine.exe',
			'/quiet',
			CustomMessage('ace_title'),
			CustomMessage('ace_size'),
			ace_url,false,false);
   end;
end;