CREATE TABLE CurrentDownNodes (
    IPAddress varchar(15),
    Name varchar(100),
    Area varchar(50),
    DownTime DATETIME,
    PRIMARY KEY (IPAddress) 
);

CREATE TABLE Nodes_Status (
    IPAddress varchar(15),
    Name varchar(100),
    Area varchar(50),
    DownTime DATETIME,
	UpTime DATETIME,
	DownDuration_ddhhmm varchar(11),
	Down_TotalHour INT,
	Down_Min INT,
	Month_Cycle INT,
	Date_Cycle INT
	)
    PARTITION BY RANGE(Month_Cycle) (
        PARTITION PAT_202010 VALUES LESS THAN (202011),
        PARTITION PAT_202011 VALUES LESS THAN (202012),
        PARTITION PAT_202012 VALUES LESS THAN (202101),
        PARTITION PAT_202101 VALUES LESS THAN (202102)
	);
	


-- ALTER table Nodes_status drop partition pat_max;

-- ALTER TABLE Nodes_status ADD PARTITION (PARTITION PAT_202101 VALUES LESS THAN (202102));