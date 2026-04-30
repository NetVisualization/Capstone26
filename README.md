# Network Visualization Capstone

---
## NOTE FOR FUTURE TEAM
As of 4/30/26, The project repository contains a file under the documentation folder containing sensitive usernames and passwords for infrastructure relating to the project. This information should be moved before sharing this project with others.

## 2025-2026 Team Members 

- Devyn Myles 

- Emile Olivier 

- Jack West 

- Tomis Hamilton 

 

## Background 

This network visualization project aims to display real-time network nodes and connections within a VR environment. Particularly, given input .pcap or .pcapng data, the application will parse the data into a database to be pulled and rendered logically within the display. 

 

## Program Architecture 

![Project Screenshot](NetVis_architecture.png) 



# Initial Setup and Main Components 

The implementation and usage of this application requires a properly set-up development environment. This section will list each component required, with other following sections and separate .md files going on more in-depth walkthroughs to configure the proper settings. 

We recommend configuring the backend components before setting up the frontend. 


## First Things First: Backend Setup

To begin, you must have an operational Linux server (stand-alone or otherwise). The documentation needed to accomplish this can be found at: https://iritt.medium.com/setting-up-ubuntu-on-vmware-for-your-cybersecurity-home-lab-ccfa0e450e46
 
Within your Linux server, you can begin the setup of the backend architecture
1. On your system, run install.sh (found in parser/scripts). This sets up the Clickhouse database and the Zeek IDS capabilities.
2. To capture network traffic for analysis within the program, run parser.sh (also located in parser/scripts).


### GitLab 

Our project repository resides on GitLab, which is another collaborative platform similar to GitHub. An account can be easily created through an existing GitHub account. This repository is also hosted on our internal server (Cyber-Team). 

To access, you will need to be connected to the Cyber-Team Wi-fi. The web page can be accessed by using the URL "gitlab.netvis.local". Our project (capstone26) resides in our group (capstoneEliteEmployees); becoming a member of the group should gain you access to the repository within capstone26. The repository can be treated like a standard GitHub repo when it comes to cloning to a system, push/pull/commits, etc. 

 

### Unity Hub/Unity Editor 

 These programs help to execute the program and host the VR display. Installing Unity Hub will automatically install Unity Editor, with Unity Hub being the launcher/manager of projects and Unity Editor allowing editing to the layout of the VR display, building the scene to display in the headset, etc. 

 

Here are the steps to properly setup the project to be opened and editable within Unity: 

1. Download and install Unity. This can be found through a simple Google search. For our purposes, we downloaded Unity Personal. 

2. Once you open Unity, navigate to the Projects tab on the far left of the screen. This should be empty. 

3. If not already done so, the project will need to be cloned onto your system.  

4. To import your project, go to the "Add" dropdown -> "Add project from disk" 

5. File explorer should pop up. Navigate to the location of the project, enter the main repository folder and select the folder named "visualizer". Upload this to Unity. If done correctly, the project should appear with the last two directories in the file path being capstone26\visualizer (unless capstone26 was changed to a different name after cloning). 

This project is currently configured to run with an older version of Unity Editor (2022.3.13f1). This separate verison will need to be downloaded manually before the project can be run.
1. Navigate to "Installs" on Unity Hub. Search for "2022.3.13f1" in the search bar. Download this version. NOTE: since this is an older version, security vulnurabilities exist.
2. In the "Projects" section, toggle the editor version so that 2022.3.13f1 is selected.


The project is now ready to be run within Unity Editor. By clicking on the project within Unity Hub, it will begin to be opened by Unity Editor. If the project is being opened for the first time, this will likely take awhile. 

A popup will likely appear during project loadup noting compile errors and the ability to open Safe Mode. These compile errors will come from NuGet Unity packages not being fully installed. For this, you can select "Ignore" and allow Unity Editor to complete the remainder of its loadup. Once Unity Editor is opened:
1. Find the "NuGet" dropdown at the top of the screen -> Manage NuGet Packages
2. Find Clickhouse.client and ZstdSharp.Port using the search bar and install both.
3. If compile errors do not clear, you will need to reopen the project in the editor.

 

To run the scene within Unity Editor: 

1. Select the "Project" folder in the bottom left side of the editor. There should be a file directory that appears below this on the left side. 
2. Within the "Assets" directory, navigate to the "Scenes" directory. Within this should be a file named "Main Scene". Click this. This step is necessary in order for the project to run within Unity. 
3. To run the scene, click on the play button located above the environment display. The scene can be paused and resumed with the adjacent buttons. 

 
If the backend is properly set-up, nodes should appear in the VR environment when the scene is run.
 

### Headset (Meta Quest Pro) 

The headset will allow the program's user to view and interact with the connections and nodes within the VR environment. The headset will need to have its settings configured in a certain way in order to be compatible with Unity (explained later). 

 

### Docker Desktop 

Docker Desktop helps deploy the database as well as the live traffic capture capability. In most simple, small-scale setups and usages, Docker allows you to setup the entire backend of the program to the same host that servers as the frontend visualization. The Docker Desktop app will need to be downloaded, however, the bulk of the setup can be done from the OS terminal. 

The specific steps to setup Docker are located in : [Link Text](documentation\docker-setup.md)

 

### Parser (Rust) 

The parser is coded in Rust and is responsible for parsing the live data traffic from the input pcap/pcapng file and bulk inserting into the database. Within the live capture ingest documentation file, you will find more specific details on how to download the necessary compiler/tools to configure and run the parser. 

 

### Database (Clickhouse) 

The database stores the information parsed from the input pcap file. We utilized Clickhouse for its implementation. In our case, we hosted our database on a local web server utilizing port 8123. In this configuration, we can access the database directly through the URL http://[your_IP]:8123 while on our local internet source (though USAFA firewall/security configurations may need to be altered to accomplish this) 

 

### IDS Setup and Zeek Configuration:

Zeek provides Intrusion Detection Capabilities that can be rendered by the visualization​.

Red nodes: specific alerts configured by the user were triggered​

Yellow Nodes: truncated or malformed packets, indicating network bottlenecks or malicious
activity

![Project Screenshot](alerts.png)
