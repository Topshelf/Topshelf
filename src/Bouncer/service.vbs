Set servicelist= GetObject("winmgmts:").InstancesOf("Win32_Service")

for each service in servicelist
	sname=lcase(service.name)
	If left(sname, 5) = "stuff" Then
		service.delete 
	end if
next