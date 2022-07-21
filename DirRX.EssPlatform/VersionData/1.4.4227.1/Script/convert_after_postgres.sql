DO $$
begin
  -- Заполнить свойство Адрес Соглашения об ЭДО справочника Настройки подключения к сервисам личного кабинета.
	if exists (SELECT * FROM information_schema.columns
              WHERE table_name = 'dirrx_esspl_esssetting' and column_name = 'agreementurl')
  then
	  update
      DirRX_EssPl_EssSetting
    set
      AgreementUrl = 'https://www.directum.ru/download/agreement_information_system.pdf'
    where
      AgreementUrl is null
      and IsUsedIdentity = 'True';
	end if;
end$$;