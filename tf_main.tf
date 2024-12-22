terraform {
  required_version = ">=1.10.0"
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "4.14.0"
    }
  }
}

provider "azurerm" {
  subscription_id = var.subscription_id
  features {
  }
}

resource "azurerm_resource_group" "main" {
  name     = "localvenue-${var.azure_name_suffix}"
  location = "North Europe" #this location is used for all other locations
}

resource "azurerm_service_plan" "main" {
  name                = "localvenue-${var.azure_name_suffix}"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  os_type             = "Windows"
  sku_name            = "F1"
}

output "resource_group_name" {
  value = azurerm_resource_group.main.name
}
