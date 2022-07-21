DO $$
begin

-- Заполнить свойство Соглаcие на КЭДО в справочнике Сотрудники.
	if exists (SELECT * 
	           FROM information_schema.columns
             WHERE table_name = 'sungero_core_recipient' and 
                   column_name = 'consentdirrx_hrsolut_dirrx')
  then
	-- Для сотрудников, которые подключены к ЛК, проставить Подписано, для остальных записей - Не подписано.
  UPDATE Sungero_Core_Recipient
    SET ConsentDirRX_HRSolut_DirRX =
      CASE 
        WHEN PersAccStatus_EssPlS_DirRX = 'InviteAccepted' THEN 'Signed' 
        ELSE 'NotSent'
      END
    WHERE Discriminator = 'b7905516-2be5-4931-961c-cb38d5677565'
          and ConsentDirRX_HRSolut_DirRX is NULL;
	end if;
end$$;