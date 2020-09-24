CREATE TABLE CurrentDownNodes (
    IPAddress varchar(15),
    Name varchar(100),
    Area varchar(50),
    DownTime DATETIME,
    PRIMARY KEY (IPAddress) 
);