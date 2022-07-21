-- Заполнить свойство Адрес Соглашения об ЭДО справочника Настройки подключения к сервисам личного кабинета.
if exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS
           WHERE TABLE_NAME = 'DirRX_EssPl_EssSetting' AND COLUMN_NAME = 'AgreementUrl')
begin 
  update
    DirRX_EssPl_EssSetting
  set
    AgreementUrl = 'https://www.directum.ru/download/agreement_information_system.pdf'
  where
    AgreementUrl is null
    and IsUsedIdentity = 'True'
end