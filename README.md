# sfdc-package-to-txt
List all types within a package.xml provided into a txt. It is a command line application.

## Usage
Just call the application without arguments having the defaults in the app.config file. You may as well pass in arguments from the command line.

### Example without argumnets

This will get the default arguments from the app.config file. In the folder "sfdc-src-project1" must exists a package.xml file, the program should generate a Components.txt.

```bash
C:\sfdc-src-project1\pacakgetotxt
```
### Example with arguments

This will get all the xml in the folder C:\sfdc-src-project1 where the name ends with "pacakge.xml" and will generate a Components.txt file for each of them in the c:\ path.
```bash
C:\pacakgetotxt --inputdir C:\sfdc-src-project1 --pattern *package.xml
```


## TODO 
Better documentation, listing and explaining each arguments.
