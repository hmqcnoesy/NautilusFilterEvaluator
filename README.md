# NautilusFilterEvaluator
Evaluates filter speed for Nautilus environment

Requires `filter_evaulation` table in schema specified in connection string:

```sql
create table filter_evaluation (
  id number,
  data_source varchar2(255),
  filter_id number,
  name varchar2(255),
  status varchar2(255),
  time number,
  sql_text varchar2(4000),
  timestamp date,
  comments varchar2(2000));
  
create sequence sq_filter_evaluation;
```

Args:

1. Connection string
2. Starting filter ID (usually 1)
3. Comments to include with each evaluation record

Example usage:

```shell
NautilusFilterEvaluator.exe "Data Source=nautilus;User Id=lims_sys;Password=*;" 1 "comment for this run"
```

Multiple runs queued up in a bat file:

```shell
NautilusFilterEvaluator.exe "Data Source=nautilus;User Id=lims_sys;Password=*;" 1 "run1"
NautilusFilterEvaluator.exe "Data Source=nautilus;User Id=lims_sys;Password=*;" 1 "run2"
NautilusFilterEvaluator.exe "Data Source=nautilus;User Id=lims_sys;Password=*;" 1 "run3"
```

Example SQL for comparing two evaluations' broken filters:

```sql
--Broken filters
select nvl(ds1.filter_id, ds2.filter_id) filter_id,
    nvl(ds1.name, ds2.name) name,
    ds1.status ds1_status,
    ds2.status ds2_status,
    ds1.min_time ds1_min_time,
    ds2.min_time ds2_min_time,
    ds1.max_time ds1_max_time,
    ds2.max_time ds2_max_time,
    round(ds1.avg_time, 0) ds1_avg_time,
    round(ds2.avg_time, 0) ds2_avg_time,
    ds1.ct ds1_ct,
    ds2.ct ds2_ct
  from 
    (select data_source, filter_id, name, status, max(time) max_time, min(time) min_time, avg(time) avg_time, count(time) ct
    from  filter_evaluation 
    where data_source = :data_source1
    group by data_source, filter_id, name, status) ds2
  full outer join
    (select data_source, filter_id, name, status, max(time) max_time, min(time) min_time, avg(time) avg_time, count(time) ct
    from  filter_evaluation 
    where data_source = :data_source2
    group by data_source, filter_id, name, status) ds1
  on ds1.filter_id = ds2.filter_id
  where ds1.status != 'ok' or ds2.status != 'ok'
order by 1;
```

Example SQL comparing speed of filters in two evaluations:

```sql
--Timing comparison
select ds1.filter_id ds1_id, ds2.filter_id ds2_id,
    nvl(ds1.name, ds2.name) name,
    ds1.status ds1_status,
    ds2.status ds2_status,
    ds1.ct ds1_ct,
    ds1.min_time ds1_min_time,
    ds1.max_time ds1_max_time,
    round(ds1.avg_time, 0) ds1_avg_time,
    ds2.ct ds2_ct,
    ds2.min_time ds2_min_time,
    ds2.max_time ds2_max_time,
    round(ds2.avg_time, 0) ds2_avg_time,
    case when ds1.avg_time < 2000 and ds2.avg_time < 2000 then 'N/A'
      when ds1.avg_time < ds2.avg_time then to_char(round((ds1.avg_time - ds2.avg_time) / ds1.avg_time, 1))
      else to_char(round((ds1.avg_time - ds2.avg_time) / ds2.avg_time, 1)) end ds1_to_ds2
  from 
    (select data_source, filter_id, name, status, max(time) max_time, min(time) min_time, avg(time) avg_time, count(time) ct
    from  filter_evaluation 
    where data_source = :data_source1
    group by data_source, filter_id, name, status) ds1
  join
    (select data_source, filter_id, name, status, max(time) max_time, min(time) min_time, avg(time) avg_time, count(time) ct
    from  filter_evaluation 
    where data_source = :data_source2
    group by data_source, filter_id, name, status) ds2
  on ds1.name = ds2.name
  where nvl(ds1.status, 'ok') = 'ok' and nvl(ds2.status, 'ok') = 'ok'
order by 1;
```
