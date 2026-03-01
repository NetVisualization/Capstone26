FROM zeek/zeek:latest

# y'all will have to compile zeek packages from source
RUN apt-get update && apt-get install -y \
    cmake make gcc g++ libpcap-dev libssl-dev python3-pip git cron wget \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*

RUN zkg autoconfig --force
# install some community packages for a general baseline of network visibility, add more here
RUN zkg install --force \
    zeek/salesforce/ja3 \
    zeek/corelight/hassh \
    zeek/mitre-attack/bzar

# download hashes from abuse.ch and convert to zeek intel format, configure to update daily
COPY zeek/config/update-intel.sh /usr/local/bin/update-intel.sh
RUN chmod +x /usr/local/bin/update-intel.sh
RUN /usr/local/bin/update-intel.sh
RUN echo "0 0 * * * root /usr/local/bin/update-intel.sh" > /etc/cron.d/zeek-intel && \
    chmod 0644 /etc/cron.d/zeek-intel && \
    crontab /etc/cron.d/zeek-intel

COPY zeek/config/zeek-entrypoint.sh  /usr/local/bin/entrypoint.sh
RUN chmod +x /usr/local/bin/entrypoint.sh
ENTRYPOINT ["/usr/local/bin/entrypoint.sh"]
CMD ["zeek", "-C", "local"] 