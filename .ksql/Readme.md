## How to execute per cluster
- put all script `ALL_01_POSTAL_SNAPSHOT_OSLO_STREAM` into ksqlDB
- Set auto.offset.reset = earliest
- execute
- now do the same for the specific script(s)

## How to set up the connectors
- put the connectors script into ksqlDB
- replace *** with the secrets from last pass
- Run the script