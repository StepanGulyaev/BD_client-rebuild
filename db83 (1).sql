--1.таблица Справочник
CREATE TABLE Spravochnik (
	sprav_id SERIAL PRIMARY KEY, -- id справочника
	sprav_name TEXT, -- название участка
	sprav_amountoffreeland int, --кол-во свободной земли
	sprav_amountofoccupiedland int --кол-во занятой земли
);

--2.таблица Владельца
CREATE TABLE Owners (
	own_id SERIAL PRIMARY KEY UNIQUE, --id владельца
	own_fio TEXT NOT NULL UNIQUE,
	own_facetype TEXT NOT NULL, --лицо
	own_communicationmethod TEXT, --способ связи
	CONSTRAINT  CHK_Own CHECK ( (own_facetype = 'физическое') or (own_facetype = 'юридическое') ),
	own_own INT REFERENCES Owners (own_id)
);

--3.таблица Участка
CREATE TABLE Region (
	reg_id SERIAL PRIMARY KEY, --id участка
	reg_name TEXT NOT NULL, --наименование участка
	reg_square INT NOT NULL, --площадь в метрах кв.
	reg_address TEXT, --адрес
	reg_amountregion INT, --кол-во объектов
	reg_cadastralobject INT, --кадастровый номер
	reg_sprav INT REFERENCES Spravochnik (sprav_id) ON DELETE CASCADE, --ID справочника
	reg_tax TEXT, --налог есть\нет
	reg_costmeter DECIMAL (12,2), --стоимость за 1 метр
	reg_sum DECIMAL (12,2), --итоговая сумма
	CONSTRAINT CHK_Reg CHECK ( (reg_tax = 'да') or (reg_tax = 'нет') )

);

--4.таблица Объекта
CREATE TABLE Object (
	obj_id SERIAL PRIMARY KEY,--id объекта
	obj_name TEXT NOT NULL , --наименование объекта
	obj_salary decimal(12,2), --цена за метр кв.
	obj_square INT, -- площадь в метрах кв.
	obj_dateofcommissioning DATE, --дата ввода в эксплуатацию
	obj_reg INT REFERENCES Region (reg_id) ON DELETE CASCADE, --id участка
	obj_own INT REFERENCES Owners (own_id) ON DELETE CASCADE --id владельца
);


--5. Логи владельцев
CREATE TABLE logs
(
 log_id serial PRIMARY KEY,
 log TEXT
);

--функция расчета суммы
CREATE OR REPLACE FUNCTION reg_calc_sum_i()
 RETURNS trigger
AS $$
	BEGIN
		UPDATE Region
		SET reg_sum = reg_square*reg_costmeter

		WHERE reg_square >0;
							RETURN NEW;
	END;
	$$ LANGUAGE 'plpgsql';
CREATE TRIGGER tr_reg_calc_sum_i
AFTER INSERT ON Region
FOR EACH ROW WHEN (pg_trigger_depth()=0)
EXECUTE PROCEDURE reg_calc_sum_i();

CREATE OR REPLACE FUNCTION reg_calc_sum_u()
 RETURNS trigger
AS $$
	BEGIN
		UPDATE Region
		SET reg_sum = reg_square*reg_costmeter

		WHERE reg_square >0;
							RETURN NEW;
	END;
	$$ LANGUAGE 'plpgsql';
CREATE TRIGGER tr_reg_calc_sum_u
AFTER UPDATE  ON Region
FOR EACH ROW WHEN (pg_trigger_depth()=0)
EXECUTE PROCEDURE reg_calc_sum_u();

/**4. В таблице (в соответствии с вариантом) предусмотреть поле, которое заполняется
автоматически по срабатыванию триггера при добавлении, обновлении и удалении
данных, иметь возможность продемонстрировать работу триггера при работе
приложения. Триггеры должны обрабатывать только те записи, которые были
добавлены, изменены или удалены в ходе текущей операции (транзакции).
 **/
 --добавление владелецев в логи
CREATE OR REPLACE FUNCTION public.own_added()
RETURNS trigger
LANGUAGE plpgsql
AS $function$
BEGIN
	insert into logs (log) values (concat(transaction_timestamp(), concat('Добавлен новый владелец: ', new.own_fio::TEXT)));
	RETURN NEW;
	END;
$function$;
CREATE TRIGGER tr_own_added
AFTER INSERT ON Owners
FOR EACH ROW EXECUTE PROCEDURE own_added();


-- изменение владельцев в логах
CREATE OR REPLACE FUNCTION public.own_changed()
RETURNS trigger
LANGUAGE plpgsql
AS $function$
BEGIN
	insert into logs (log) values (concat(transaction_timestamp(), concat('Владелец изменён: ', new.own_fio::TEXT)));
	RETURN NEW;
END;
$function$;

CREATE TRIGGER tr_reg_changed
AFTER UPDATE OF own_fio ON Owners
FOR EACH ROW EXECUTE PROCEDURE own_changed();

--удаление владелцев в логах
CREATE OR REPLACE FUNCTION public.own_deleted()
RETURNS trigger
LANGUAGE plpgsql
AS $function$
BEGIN
	insert into logs (log) values (concat(transaction_timestamp(), concat('Владелец удалён: ', old.own_fio::TEXT)));
	RETURN NEW;
END;
$function$;

CREATE TRIGGER tr_own_deleted
BEFORE DELETE ON Owners
FOR EACH ROW EXECUTE PROCEDURE own_deleted();

--заполнение тестовыми данными табл 1
INSERT INTO Spravochnik VALUES
(1,'КК-01',1000,500),
(2,'РТ-04',2000,600),
(3,'НГ-22',6000,800);

SELECT *FROM Spravochnik;

--заполнение тестовыми данными табл 2
INSERT INTO Owners VALUES
(1,'Максимов Е.Р.','физическое','почта',1),
(2,'Рырырк Г.Г.','юридическое','телефон',2),
(3,'Мурлыкзабеткович Р.Н.','физическое','телефон',3);

SELECT *FROM Owners;
--заполнение тестовыми данными табл 3
INSERT INTO Region VALUES
(1,'КК-01',400,'Москва',5,888677,1,'да',12000),
(2,'РТ-04',250,'Воронеж',2,789521,2,'нет',7000),
(3,'НГ-22',700,'Крым',9,112255,3,'нет',80000);

SELECT *FROM Region;

--заполнение тестовыми данными табл 4
INSERT INTO Object VALUES
(1,'КК-01',12000,400,'2015-07-23',1,1),
(2,'РТ-04',7000,250,'2018-06-30',2,2),
(3,'НГ-22',80000,700,'2019-08-17',3,3);

SELECT *FROM Object;

--Создать индексы для увеличения скорости выполнения запросов
CREATE INDEX idx_reg ON Region USING hash (reg_sum)  ;
CREATE INDEX idx_obj ON Object USING btree (obj_name, obj_salary, obj_square, obj_dateofcommissioning, obj_reg) ;

--Запросы:
--a. Составной многотабличный запрос с параметром, включающий соединение таблиц и CASE-выражение;
--вывод фио владельца и суммы налога
SELECT
	obj_name AS "Участок",
		 CASE
								WHEN reg_tax = 'да' THEN 5000
								WHEN reg_tax = 'нет' THEN 0000
								ELSE 0
							END
			AS "Налог"
FROM
	Object
INNER JOIN Region on Region.reg_id = Object.obj_reg ;

--b. На основе обновляющего представления (многотабличного VIEW), в котором критерий упорядоченности задает пользователь при выполнении;
--вывод id участка, название, кол-во свободной земли и город
CREATE VIEW public.reg_svobodno as
SELECT
 reg_id, reg_name, sprav_amountoffreeland, reg_address

FROM
 Spravochnik, Region
WHERE reg_sprav = sprav_id;

SELECT *FROM reg_svobodno;


--c. Запрос, содержащий коррелированные и некоррелированные подзапросы в разделах SELECT, FROM и WHERE (в каждом хотя бы по одному);
--вывод id участка, его наименование, фио владельца и кол-во занятой земли
SELECT reg_id, reg_name,
	(SELECT own_fio 
	 FROM Owners
	 WHERE own_id = reg_own),
	 
	(SELECT sprav_amountofoccupiedland 
	 FROM Spravochnik
	 WHERE reg_sprav = reg_id)
FROM 
	(SELECT *FROM Region
		 WHERE reg_id != 0) AS Zemliy
	WHERE reg_address = 'Москва';
------
SELECT reg_id,
	(SELECT obj_square  
	 FROM Object
	 WHERE obj_id = obj_reg),
	 
	(SELECT sprav_amountofoccupiedland 
	 FROM Spravochnik
	 WHERE reg_sprav = reg_id)
FROM 
	(SELECT reg_id FROM Region
	 WHERE reg_id != 0) AS Zemliy
WHERE reg_address = 'Москва';

--d. Многотабличный запрос, содержащий группировку записей, агрегативные функции и параметр, используемый в разделе HAVING;
-- вывод ценник за землю и город, дороже 1750000.00
SELECT  max(reg_sum), reg_address
FROM Region
WHERE reg_costmeter >0
GROUP BY reg_id
HAVING max(reg_sum)>1750000.00;

--e. Запрос, содержащий предикат ANY(SOME) или ALL
--вывод наименование участка,свободная земля, дороже 1750000.00
SELECT
	sprav_name, sprav_amountoffreeland
FROM Spravochnik
WHERE sprav_id = any
			(SELECT reg_sprav FROM Region WHERE reg_sum>1750000.00);

/*В запросе (из пункта 2 или в дополнительном к тому перечню)
использовать собственную скалярную функцию, а в хранимой
процедуре – векторную (или табличную) функцию. Функции
сохранить в базе данных.**/
-- Скалярная функция
CREATE OR REPLACE FUNCTION public.print_own (_id INT, _fio TEXT)
RETURNS TEXT
LANGUAGE plpgsql
AS $function$ DECLARE
OwnFio TEXT;
BEGIN
	SELECT own_facetype
	INTO OwnFio
	FROM Owner
	WHERE own_id = _id;
	RETURN OwnFio;
END; $function$;

-- Табличная функция
CREATE OR REPLACE FUNCTION public.print_region ()
RETURNS TABLE (_id int, _tax text, _costmeter decimal, _sum decimal )
LANGUAGE sql
AS $function$
 SELECT reg_id, reg_tax, reg_costmeter, reg_sum FROM
Region WHERE reg_address = 'Москва';
 $function$;


/*Реализовать отдельную хранимую процедуру, состоящую из
нескольких отдельных операций в виде единой транзакции,
которая при определенных условиях может быть зафиксирована
или откатана. **/
CREATE OR REPLACE PROCEDURE public.comrol_reg ()
LANGUAGE plpgsql
AS $function$ DECLARE
BEGIN
	DELETE FROM Region
	WHERE reg_address = 'Крым';
	COMMIT;
END;
 $function$;

CREATE OR REPLACE PROCEDURE public.rollback_reg ()
LANGUAGE plpgsql
AS $function$ DECLARE
BEGIN
	DELETE FROM Region
	WHERE reg_address = 'Крым';
	ROLLBACK;
END; $function$;

/*Распределение прав пользователей: предусмотреть не менее двух
пользователей с разным набором привилегий. Каждый набор
привилегий оформить в виде роли. **/

CREATE ROLE Direction_admim;
CREATE ROLE Prodavec;
GRANT SELECT, UPDATE, INSERT, DELETE ON logs, Owners, Object, Region, Spravochnik TO Direction_admim;
GRANT USAGE ON SEQUENCE logs_log_id_seq TO Direction_admim;
GRANT SELECT ON reg_svobodno to Direction_admim;
GRANT SELECT ON logs, Owners, Object, Region, Spravochnik, reg_svobodno TO Prodavec;
CREATE USER Andrey with password 'postgres';
CREATE USER Sania with password 'pg4321';
GRANT Direction_admim TO Andrey;
GRANT Prodavec TO Sania;

/*Операции добавления, удаления и обновления реализовать в виде
хранимых процедур (с параметрами) хотя бы для одной таблицы;
для остальных допустимо использовать возможности связывания
полей ввода в приложении с полями БД.**/
--добавление владельца
CREATE OR REPLACE FUNCTION public.add_owners(_fio text, _facetype text, _communicationmethod text, _own INT)
RETURNS boolean
LANGUAGE plpgsql
AS $function$ DECLARE
return_val BOOLEAN := true;
own_count integer := 0;
BEGIN
	if (_fio IS null)
	THEN
	raise EXCEPTION 'FIO cannot be an empty field!';
	return_val := false;
	END if;

	if (_facetype IS null)
	THEN
	raise EXCEPTION 'Type of face cannot be empty!';
	return_val := false;
	END if;

	if (_communicationmethod IS null)
	THEN
	raise EXCEPTION 'communication method date cannot be an empty field!';
	return_val := false;
	END if;
	
	if (_own IS null)
	THEN
	raise EXCEPTION 'FK cannot be an empty field!';
	return_val := false;
	END if;


	BEGIN
		SELECT count(*)
		INTO own_count
		FROM Owners
		WHERE own_fio = _fio;
		end;

		if (own_count > 0)
		THEN
		raise EXCEPTION 'Full air craft already exists!';
		return_val := false;
		END if;

	if (return_val = TRUE)
	THEN
	INSERT INTO Owners (own_fio, own_facetype, own_communicationmethod, own_own)
	VALUES (_fio, _facetype, _communicationmethod,_own);
	END if;

RETURN return_val;
END; $function$ ;

--изменение владельца
CREATE OR REPLACE FUNCTION public.change_own (_oldfio text, _newfio text, _newfacetype text, _newcommunicationmethod text,_own int)
RETURNS boolean
LANGUAGE plpgsql
AS $function$declare
return_val boolean := true;
own_check int;
BEGIN
	begin
	select count(*)
	into own_check
	from Owners
	where own_fio = _newfio;
	end;

	if (_oldfio = _newfio)
	THEN
	own_check = own_check -1 ;
	end if;



	begin
	select count(*)
	into own_check
	from Owners
	where  own_fio = _oldfio;
	end;

	if (own_check > 0)
	then
	raise exception 'Already exists';
	return_val = false;
	end if;

	if (return_val = true)
	THEN
	update Owners
	set own_fio = _newfio,
	own_facetype = _newfacetype,
	own_communicationmethod = _newcommunicationmethod,
	own_own = _own
	where own_fio= _oldfio;
	end if;

return return_val;
END; $function$ ;

--удаления владельца
create function public.delete_own(_oldfio text)
returns boolean
language plpgsql
as $$ declare
return_val boolean := true;
own_check int;
BEGIN
    select count(*)
    into own_check
    from Owners
    where own_fio = _oldfio;
    if (own_check = 0)
    then
    raise exception 'There are no FIO with this number!';
    return_val = false;
    end if;

    if (own_check = 1)
    then
    DELETE from Owners
    where own_fio = _oldfio;
    end if;

return return_val;
END;
$$;

--добавление участка
create function add_reg(_name text, _square int, _address text, _amountregion int, _cadastralobject int, _sprav int, _tax text, _costmeter numeric)
returns boolean
    language plpgsql
as
$$
DECLARE
return_val BOOLEAN := true;
reg_count integer;
BEGIN
    if (_name IS null)
        THEN
        raise EXCEPTION 'name cannot be an empty field!';
        return_val := false;
    END if;

    if (_square IS null)
        THEN
        raise EXCEPTION 'Square can not be an empty field!';
        return_val := false;
    END if;

    if (_address IS null)
        THEN
        raise EXCEPTION 'address date can not be an empty field!';
        return_val := false;
    END if;

    if (_amountregion IS null)
        THEN
        raise EXCEPTION 'amount region  can not be an empty field!';
        return_val := false;
    END if;

    if (_cadastralobject IS null)
        THEN
        raise EXCEPTION 'cadastral number can not be an empty field!';
        return_val := false;
    END if;

    if (_own IS null)
        THEN
        raise EXCEPTION 'ID sprav can not be an empty field!';
        return_val := false;
    END if;

    if (_tax IS null)
        THEN
        raise EXCEPTION 'tax can not be an empty field!';
        return_val := false;
    END if;

    if (_costmeter IS null)
        THEN
        raise EXCEPTION 'Cost meter can not be an empty field!';
        return_val := false;
    END if;



BEGIN
SELECT count(*)
INTO reg_count
FROM Region
WHERE reg_name = _name;
end;

if (reg_count > 0)
THEN
raise EXCEPTION 'Region with this name already exists!';
return_val := false;
END if;

if (return_val = TRUE)
THEN
INSERT INTO Region (reg_name, reg_square, reg_address, reg_amountregion, reg_cadastralobject, reg_sprav, reg_tax, reg_costmeter) VALUES
(_name, _square, _address, _amountregion, _cadastralobject, _sprav, _tax, _costmeter);
END if;
RETURN return_val;
END;
$$;

--изменение участка
create function change_reg(_oldname text, _newname text, _newsquare int, _newaddress text, _newamountregion int, _newcadastralobject int,
                            _newsprav int, _newtax text, _newcostmeter numeric)
returns boolean
    language plpgsql
as
$$
declare
return_val boolean := true;
reg_check int;
BEGIN
    begin
    select count(*)
    into reg_check
    from Region
    where reg_name = _newname;
    end;

    if (_oldname = _newname)
        THEN
        reg_check = reg_check -1 ;
    end if;

    if (reg_check > 0)
        then
        raise exception 'A Name region with this name already exists!';
        return_val = false;
    end if;

if (return_val = true)
THEN
update Region
set reg_name = _newname,
reg_square = _newsquare,
reg_address = _newaddress,
reg_amountregion = _newamountregion,
reg_cadastralobject = _newcadastralobject,
reg_sprav = _newsprav,
reg_tax = _newtax,
reg_costmeter = _newcostmeter
where reg_name = _oldname;
end if;
return return_val;
END;
$$;

--удаления участка
create function delete_reg(_oldname text)
returns boolean
    language plpgsql
as
$$
declare
return_val boolean := true;
reg_check int;
BEGIN
    select count(*)
    into reg_check
    from Region
    where reg_name = _oldname;
    if (reg_check = 0)
    then
    raise exception 'There is no name region with this ID!';
    return_val = false;
    end if;

    if (reg_check = 1)
    then
    DELETE from Region
    where reg_name = _oldname;
    end if;

return return_val;
END;
$$;

--добавление справочника
create function add_sprav(_name text, _amountoffreeland int, _amountofoccupiedland int)
returns boolean
    language plpgsql
as
$$
DECLARE
return_val BOOLEAN := true;
sprav_count integer;
BEGIN
     if (_name IS null)
    THEN
    raise EXCEPTION 'name can not be an empty field!';
    return_val := false;
    END if;

    if (_amountoffreeland IS null)
    THEN
    raise EXCEPTION 'amount of free land can not be an empty field!';
    return_val := false;
    END if;

    if (_amountofoccupiedland IS null)
    THEN
    raise EXCEPTION 'amount of occupied land can not be an empty field!';
    return_val := false;
    END if;


BEGIN
    SELECT count(*)
    INTO sprav_count
    FROM Spravochnik
    WHERE sprav_name = _name;
    end;

    if (sprav_count > 0)
    THEN
    raise EXCEPTION 'spravochnik with this name already exists!';
    return_val := false;
    END if;

    if (return_val = TRUE)
    THEN
    INSERT INTO Spravochnik (sprav_name, sprav_amountoffreeland, sprav_amountofoccupiedland)
    VALUES (_name, _amountoffreeland, _amountofoccupiedland);
    END if;

RETURN return_val;
END;
$$;

--изменение справочника
create function change_sprav(_oldname text, _newname text, _newamountoffreeland int, _newamountofoccupiedland int)
returns boolean
    language plpgsql
as
$$
declare
return_val boolean := true;
sprav_check int;
BEGIN
    begin
    select count(*)
    into sprav_check
    from Spravochnik
    where sprav_name = _newname;
    end;

    if (_oldname = _newname)
    THEN
    sprav_check = sprav_check -1 ;
    end if;

    if (sprav_check > 0)
    then
    raise exception 'Spravochnik with that name already exists!';
    return_val = false;
    end if;

    if (return_val = true)
    THEN
    update Spravochnik
    set sprav_name = _newname,
    sprav_amountoffreeland = _newamountoffreeland,
    sprav_amountofoccupiedland = _newamountofoccupiedland
    where sprav_name = _oldname;
    end if;

return return_val;
END;
$$;

--удаление справочника
create function delete_sprav(_oldname int) returns boolean
    language plpgsql
as
$$
declare
return_val boolean := true;
sprav_check int;
BEGIN
    select count(*)
    into sprav_check
    from Spravochnik
    where sprav_name = _oldname;
    if (sprav_check = 0)
    then
    raise exception 'Spavochnik is impossible without working days!';
    return_val = false;
    end if;

    if (sprav_check = 1)
    then
    DELETE from Spravochnik
    where sprav_name = _oldname;
    end if;

return return_val;
END;
$$;

--добавление объекта
CREATE OR REPLACE FUNCTION public.add_obj(_name text, _salary numeric, _square int, _dateofcommissioning date, _reg int, _own int)
RETURNS boolean
LANGUAGE plpgsql
AS $function$DECLARE
return_val BOOLEAN := true;
obj_count integer := 0;
BEGIN
	if (_name IS null)
	THEN
	raise EXCEPTION 'Name cannot be an empty field!';
	return_val := false;
	END if;

	if (_salary IS null)
	THEN
	raise EXCEPTION 'Salary cannot be empty!';
	return_val := false;
	END if;

	if (_square IS null)
	THEN
	raise EXCEPTION 'Square cannot be empty!';
	return_val := false;
	END if;

	if (_dateofcommissioning IS null)
	THEN
	raise EXCEPTION 'date of commissioning cannot be empty!';
	return_val := false;
	END if;

	if (_reg IS null)
	THEN
	raise EXCEPTION 'id region cannot be empty!';
	return_val := false;
	END if;
	
	if (_own IS null)
	THEN
	raise EXCEPTION 'id owner cannot be empty!';
	return_val := false;
	END if;


	BEGIN
		SELECT count(*)
		INTO obj_count
		FROM Object
		WHERE obj_name = _name;
		end;

		if (obj_count > 0)
		THEN
		raise EXCEPTION 'Full name already exists!';
		return_val := false;
		END if;



	if (return_val = TRUE)
	THEN
	INSERT INTO Object ( obj_name, obj_salary, obj_square, obj_dateofcommissioning, obj_reg, obj_own)
	VALUES (_name, _salary, _square, _dateofcommissioning, _reg, _own);
	END if;

RETURN return_val;
END; $function$ ;

--изменение объекта
CREATE OR REPLACE FUNCTION public.change_obj(_oldname text, _newname text, _newsalary numeric, _newsquare int, 
											 _newdateofcommissioning date, _newreg int, _newown int)
RETURNS boolean
LANGUAGE plpgsql
AS $function$declare
return_val boolean := true;
obj_check int;
BEGIN
	begin
	select count(*)
	into obj_check
	from Object
	where obj_name = _newname;
	end;

	if (_oldname = _newname)
	THEN
	obj_check = obj_check -1 ;
	end if;

	begin
	select count(*)
	into obj_check
	from Object
	where obj_name = _oldname;
	end;

	if (obj_check > 0)
	then
	raise exception 'Already exists';
	return_val = false;
	end if;

	if (return_val = true)
	THEN
	update Object
	set obj_name = _newname,
	obj_salary = _newsalary,
	obj_square = _newsquare,
	obj_dateofcommissioning = _newdateofcommissioning,
	obj_reg = _newreg,
	obj_own = _newown
	where obj_name = _oldname;
	end if;

return return_val;
END; $function$ ;

--удаление объекта
create function public.delete_obj(_oldname text)
returns boolean
language plpgsql
as $$ declare
return_val boolean := true;
obj_check int;
BEGIN
    select count(*)
    into obj_check
    from Object
    where obj_name = _oldname;
    if (obj_check = 0)
    then
    raise exception 'There are no object with this ID!';
    return_val = false;
    end if;

    if (obj_check = 1)
    then
    DELETE from Object
    where obj_name = _oldname;
    end if;

return return_val;
END;
$$;

--В триггере или хранимой процедуре реализовать курсор
--на обновление отдельных данных
CREATE OR REPLACE FUNCTION public.Region(_id INT, _newname text,_newsquare INT, _newaddress TEXT, _newamountregion INT,
										_newcadastralobject INT,_newsprav INT, _newtax TEXT,_newcostmeter numeric)
RETURNS boolean
LANGUAGE plpgsql
AS $function$ declare
return_val boolean := true;
new_sprav int = -1;

sprav_cursor CURSOR (firstCursor TEXT) FOR SELECT sprav_id FROM Spravochnik
WHERE sprav_amountofoccupiedland = firstCursor FOR UPDATE;

BEGIN
OPEN sprav_cursor (firstCursor :=  _newsprav);
FETCH sprav_cursor INTO new_sprav;

if (new_sprav = -1)
then
return_val = false;
end if;

if (return_val = true)
THEN
update Region
set reg_name = _newname,
reg_square = _newsquare,
reg_address = _newaddress,
reg_amountregion = _newamountregion,
reg_cadastralobject = _newcadastralobject,
reg_sprav = new_sprav,
reg_tax = _newtax,
reg_costmeter = _newcostmeter
where reg_id = _id;
end if;

return return_val;
END; $function$;