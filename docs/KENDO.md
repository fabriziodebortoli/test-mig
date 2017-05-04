## Kendo UI

TaskBuilder Web needs [**Kendo UI for Angular 2** ](http://www.telerik.com/kendo-angular-ui/) library.

Before restoring npm packages, we needs to associate the Progress NPM registry with the @progress scope.

```shell
# Associate the Progress NPM registry with the @progress scope.
$ npm login --registry=https://registry.npm.telerik.com/ --scope=@progress
```
NPM will ask you for your Telerik account credentials and an email, use:

```
Username: eitri
Password: Microarea.2017
Email: eitri@microarea.it
```

You should get this message:
```
Logged in as eitri to scope @progress on https://registry.npm.telerik.com/.
```

The actual packages will be downloaded when the npm install command will be executed (see below),
under the folder
```
\Web\client\web-form\node_modules\@progress
and
\Web\client\web-form\node_modules\@telerik
```