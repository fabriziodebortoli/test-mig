if ("%~1" == "") goto blank
	npm link %1/client/core/src && npm link %1/client/erp/src && npm link %1/client/icons/src && npm link %1/client/reporting-studio/src
	goto end
:blank
	echo usage: linktb <local_path_to_taskbuilder_web_repository>
:end