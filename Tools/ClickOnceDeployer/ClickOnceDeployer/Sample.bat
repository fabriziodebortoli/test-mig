Call ClickOnceDeployer Deploy /root "c:\development\Apps" /clean true /installation development /version release >deploy.log
Call ClickOnceDeployer UpdateDeployment /root "c:\development\Apps" /installation development /version release >updateDeployment.log
