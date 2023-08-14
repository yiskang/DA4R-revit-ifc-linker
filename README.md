# Revit IFC Linker App bundle for Autodesk APS Design Automation

[![Design Automation](https://img.shields.io/badge/Design%20Automation-v3-green.svg)](http://developer.autodesk.com/)

![Revit](https://img.shields.io/badge/Plugins-Revit-lightgrey.svg)
![.NET](https://img.shields.io/badge/.NET%20Framework-4.8-blue.svg)
[![Revit](https://img.shields.io/badge/Revit-2022|2023|2024-lightgrey.svg)](https://www.autodesk.com/products/revit/overview/)

![Advanced](https://img.shields.io/badge/Level-Advanced-red.svg)
[![MIT](https://img.shields.io/badge/License-MIT-blue.svg)](http://opensource.org/licenses/MIT)

# Description

This sample demonstrates how to link IFC files into host RVT file using `Importer` from `Revit.IFC.Import.dll` shipped with Revit software on Design Automation.

# Development Setup

## Prerequisites

1. **APS Account**: Learn how to create a APS Account, activate subscription and create an app at [this tutorial](https://aps.autodesk.com/tutorials).
2. **Visual Studio 2022 and later** (Windows).
3. **Revit 2022 and later**: required to compile changes into the plugin.

## Design Automation Setup

### AppBundle example

```json
{
    "id": "RevitIfcImporter",
    "engine": "Autodesk.Revit+2022",
    "description": "Link IFC into host RVT"
}
```

### Activity example

```json
{
    "id": "RevitIfcImporterActivity",
    "commandLine": [
        "$(engine.path)\\\\revitcoreconsole.exe /al \"$(appbundles[RevitIfcImporter].path)\""
    ],
    "parameters": {
        "hostRVT": {
            "verb": "get",
            "description": "The ifc will be linked to host",
            "required": true,
            "localName": "host.rvt"
        },
        "inputIFC": {
            "verb": "get",
            "description": "The ifc will be linked to host",
            "required": true,
            "localName": "$(inputIFC)"
        },
        "output": {
            "zip": true,
            "verb": "put",
            "description": "The link result",
            "localName": "output"
        }
    },
    "engine": "Autodesk.Revit+2022",
    "appbundles": [
        "Autodesk.RevitIfcImporter+dev"
    ],
    "description": "Activity for linking IFC to Host RVT"
}
```

### Workitem example

```json
{
    "activityId": "Autodesk.RevitIfcImporterActivity+dev",
    "arguments": {
        "hostRVT": {
            "verb": "get",
            "url": "https://developer.api.autodesk.com/oss/v2/apptestbucket/9d3be632-a4fc-457d-bc5d-9e75cefc54b7?region=US"
        },
        "inputIFC": {
            "verb": "get",
            "url": "https://developer.api.autodesk.com/oss/v2/apptestbucket/97095bbc-1ce3-469f-99ba-0157bbcab73b?region=US"
        },
        "output": {
            "verb": "put",
            "url": "https://developer.api.autodesk.com/oss/v2/apptestbucket/9d3be632-a4fc-457d-bc5d-9e75cefc54b7?region=US",
            "headers": {
                "Content-Type": "application/octet-stream"
            }
        }
    }
}
```

## License

This sample is licensed under the terms of the [MIT License](http://opensource.org/licenses/MIT). Please see the [LICENSE](LICENSE) file for full details.

## Written by

Eason Kang [@yiskang](https://twitter.com/yiskang), [Autodesk Developer Advocacy and Support](http://aps.autodesk.com)