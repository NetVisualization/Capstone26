# Demo instructions

1. Clear database by running `1_reset_db.ps1` then load in data from PCAP via `1_pcap_demo.ps1`.  
2. 1 Instructor will view the visualization in the VR and one will view it on the board. We will talk through the network layout.  
    - talk about what hosts they are seeing  
    - what is each connection, color coding, subconnections  
    - LAN vs WAN  
3. We will show the demo on the big screen. Run `3_db_view.ps1` to view the nodes and connections tables.  
4. Clear the database by running `1_reset_db.ps1` again then load `4_live_demo.ps1`  
5. Have the instructors switch the headset. Watch the network build out for a bit. Add a new host from a blank VM. Point it out once it appears. Have it initiate traffic on the LAN (ex. SSH into a victim). Watch appear. Repeat for traffic on the WAN and watch appear.  