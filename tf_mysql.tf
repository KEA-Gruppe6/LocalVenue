resource "azurerm_mysql_flexible_server" "main" {
  name                   = "localvenue-mysql-${var.azure_name_suffix}"
  resource_group_name    = azurerm_resource_group.main.name
  location               = azurerm_resource_group.main.location
  administrator_login    = var.sql_user
  administrator_password = var.sql_password
  sku_name               = "B_Standard_B1ms"
  version                = "8.0.21"
  zone                   = "2"
}

resource "azurerm_mysql_flexible_database" "main" {
  name                = var.database_name
  resource_group_name = azurerm_resource_group.main.name
  server_name         = azurerm_mysql_flexible_server.main.name
  depends_on          = [azurerm_mysql_flexible_server.main]
  charset             = "utf8"
  collation           = "utf8_unicode_ci"
}

resource "azurerm_mysql_flexible_server_firewall_rule" "allow_webapp" {
  for_each            = toset(azurerm_windows_web_app.main.outbound_ip_address_list)
  name                = "allow-webapp-${replace(each.key, ".", "-")}"
  resource_group_name = azurerm_resource_group.main.name
  server_name         = azurerm_mysql_flexible_server.main.name
  start_ip_address    = each.value
  end_ip_address      = each.value
}

locals {
  mysql_connection_string = "server=${azurerm_mysql_flexible_server.main.fqdn};port=3306;database=${var.database_name};uid=${var.sql_user};pwd=${var.sql_password}"
}

output "mysql_connection_string" {
  value = local.mysql_connection_string
}
