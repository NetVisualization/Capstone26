FROM ubuntu:22.04

RUN apt update && apt install -y dos2unix
COPY ./netvis-entry.sh /netvis/pcap2ch/netvis-entry.sh
RUN dos2unix /netvis/pcap2ch/netvis-entry.sh