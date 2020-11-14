SELECT * from PoP_status t 
where t.Month_Cycle='202010';

SELECT * from PoP_status t 
where t.Month_Cycle='202011' and t.Date_Cycle = '13';

SELECT * from PoP_status t 
where t.Month_Cycle='202010' and t.Date_Cycle between '01' and '10';

SELECT t.IPAddress, t.Name, t.Area, t.DownTime, t.UpTime, 
t.DownDuration_ddhhmm, t.Down_TotalHour, t.Down_Min from PoP_status t 
where t.Month_Cycle='202010'
order by t.IPAddress;

SELECT t.IPAddress, t.Name, t.Area, 
(sum(t.Down_TotalHour) + sum(t.Down_Min) div 60) as Total_Hour, 
MOD(sum(t.Down_Min),60) as Minute  
from PoP_status t 
where t.Month_Cycle='202010'
group by t.IPAddress
order by t.IPAddress;

SELECT * from PoP_status t 
where t.Month_Cycle='202010'
order by t.Date_Cycle;
