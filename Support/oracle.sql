
-- drop tables

DROP TABLE dbauditdetail
/
DROP TABLE dbaudit
/
DROP TABLE dbidtable
/
DROP TABLE testentity_audit
/
DROP TABLE testentity
/
DROP TABLE bsstatus
/


-- create tables
CREATE TABLE bsstatus
    (id                             NUMBER(*,0) NOT NULL,
    name                           VARCHAR2(50) NOT NULL)
/

ALTER TABLE bsstatus
ADD CONSTRAINT pk_bsstatus PRIMARY KEY (id)
USING INDEX
/


CREATE TABLE dbaudit
    (id                             NUMBER(8,0) NOT NULL,
    username                       VARCHAR2(50),
    tablename                      VARCHAR2(50),
    action                         VARCHAR2(50),
    remarks                        VARCHAR2(4000),
    createdon                      DATE)
/

ALTER TABLE dbaudit
ADD CONSTRAINT pk_dbaudit PRIMARY KEY (id)
USING INDEX
/


CREATE TABLE dbauditdetail
    (id                             NUMBER(8,0) NOT NULL,
    auditid                        NUMBER(8,0) NOT NULL,
    oldvalue                       NVARCHAR2(2000),
    newvalue                       NVARCHAR2(2000),
    changed                        NUMBER(1,0),
    fieldname                      VARCHAR2(50))
/


ALTER TABLE dbauditdetail
ADD CONSTRAINT pk_dbauditdetail PRIMARY KEY (id)
USING INDEX
/

ALTER TABLE dbauditdetail
ADD CONSTRAINT fk_dbaudit FOREIGN KEY (auditid)
REFERENCES dbaudit (id)
/


CREATE TABLE dbidtable
    (idindex                        NUMBER(5,0),
    nextid                         NUMBER(18,0))
/



CREATE TABLE testentity
    (id                             NUMBER(*,0) NOT NULL,
    loginname                      VARCHAR2(50),
    passwd                         VARCHAR2(50),
    photo                          BLOB,
    text                           CLOB,
    status                         NUMBER(2,0),
    createdon                      DATE,
    updatedon                      DATE,
    createdby                      VARCHAR2(50),
    updatedby                      VARCHAR2(50),
    addressstreet                  VARCHAR2(500),
    addresscity                    VARCHAR2(50),
    addresspostcode                VARCHAR2(50))  
/


ALTER TABLE testentity
ADD CONSTRAINT pk_testentity PRIMARY KEY (id)
USING INDEX
/

ALTER TABLE testentity
ADD CONSTRAINT fk_bsstatus FOREIGN KEY (status)
REFERENCES bsstatus (id)
/

CREATE TABLE testentity_audit
    (auditid                        NUMBER(*,0) NOT NULL,
    id                             NUMBER(*,0) NOT NULL,
    loginname                      VARCHAR2(50),
    passwd                         VARCHAR2(50),
    photo                          BLOB,
    text                           CLOB,
    status                         NUMBER(2,0),
    createdon                      DATE,
    updatedon                      DATE,
    createdby                      VARCHAR2(50),
    updatedby                      VARCHAR2(50),
    addressstreet                  VARCHAR2(500),
    addresscity                    VARCHAR2(50),
    addresspostcode                VARCHAR2(50))  
/

ALTER TABLE testentity_audit
ADD CONSTRAINT pk_testentity_audit PRIMARY KEY (auditid)
USING INDEX
/

-- insert records
-- insert records 
insert into DbIdTable (IdIndex,NextId) values (0, 1000);
insert into DbIdTable (IdIndex,NextId) values (1, 1000);


insert into BsStatus (Id, Name) values (0, 'Approved');
insert into BsStatus (Id, Name) values (1, 'Not Approved');
insert into BsStatus (Id, Name) values (2, 'Cancelled');

commit;


