  -- В Настройках заявлений заполнить поля "№" и "Фильтрация" в списках Согласующих и Исполнителей. 
  update DirRX_HRLite_Approvers 
  set Number = Numbers.RowNumber, 
  -- Сотрудников по организации не фильтровать, Группы, Роли, Нор и подразделения - фильтровать.
  Filter = case 
    when Recipients.Discriminator = 'b7905516-2be5-4931-961c-cb38d5677565'
    then 'NoFiltration'
    else 'NeedFiltration' 
    end
  from DirRX_HRLite_Approvers Approvers
  join DirRX_HRLite_StatementsSett Settings
    on Settings.Id = Approvers.StatementsSett
  join Sungero_Core_Recipient Recipients on
    Recipients.Id = Approvers.Approver
  join (select Approvers.Id, 
     ROW_NUMBER() OVER(partition by Approvers.StatementsSett order by Approvers.StatementsSett) as RowNumber 
     from DirRX_HRLite_Approvers Approvers) as Numbers
   on Numbers.Id = Approvers.Id
  where Approvers.Number is null

  update DirRX_HRLite_Performers 
  set Number = Numbers.RowNumber, 
  -- Сотрудников по организации не фильтровать, Группы, Роли, Нор и подразделения - фильтровать.
  Filter = case 
    when Recipients.Discriminator = 'b7905516-2be5-4931-961c-cb38d5677565'
    then 'NoFiltration'
    else 'NeedFiltration' 
    end
  from DirRX_HRLite_Performers Performers
  join DirRX_HRLite_StatementsSett Settings
    on Settings.Id = Performers.StatementsSett
  join Sungero_Core_Recipient Recipients on
    Recipients.Id = Performers.Performer
  join (select Performers.Id, 
     ROW_NUMBER() OVER(partition by Performers.StatementsSett order by Performers.StatementsSett) as RowNumber 
     from DirRX_HRLite_Performers Performers) as Numbers
   on Numbers.Id = Performers.Id
  where Performers.Number is null