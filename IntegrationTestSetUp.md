# RentARoom Azure Blob Service Integration Tests

  

This project includes integration tests for the `AzureBlobService`. These tests require a running Azurite instance to interact with the Azure Blob Storage emulator.

  

## Prerequisites

  

1. **Node.js and npm:**

* Azurite is typically installed using npm (Node.js package manager).

* Ensure you have Node.js and npm installed on your machine.

* You can download them from [nodejs.org](https://nodejs.org/).

  

2. **Azurite Installation:**

* Follow instructions on how to install azurite: https://learn.microsoft.com/en-us/azure/storage/common/storage-use-azurite?tabs=visual-studio%2Cblob-storage

```npm install -g azurite ```

  

  

  

## Running Azurite

  

1. **Start Azurite:**

* Open a new powershell terminal or command prompt window. 

* Run the following command to start Azurite with default settings https://learn.microsoft.com/en-us/azure/storage/common/storage-use-azurite?tabs=visual-studio%2Cblob-storage:

  

```

azurite --silent --location c:\azurite --debug c:\azurite\debug.log

```

  

* Keep this terminal window open while running the tests.

  

## Running the Azure Blob Service Integration Tests

  

1. **Open the Solution in Visual Studio:** Open your `RentARoom.sln` file in Visual Studio.

  

2. **Navigate to the Azure Blob Service Tests:** Click on RentARoom.Tests &rarr; RentARoom.IntegrationTests &rarr; AzureBlobService &rarr; AzureBlobServiceTests

  

3. **Run the Tests:** Right-click on test file and select "Run Tests."

  

## Important Notes

  

* **Connection String:**

* The integration tests are configured to use the default Azurite connection string.

* Ensure that Azurite is running with its default settings.

* **Test Data:**

* The tests use a `test-rentaroom.png` file for image uploads.

* Make sure that this file is located in the output directory of your test assembly (e.g., `bin/Debug/net8.0/`).