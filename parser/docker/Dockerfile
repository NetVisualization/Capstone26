FROM zeek/zeek:latest

# y'all will have to compile zeek packages from source
RUN apt-get update && apt-get install -y \
    cmake make gcc g++ libpcap-dev libssl-dev python3-pip git \
    && rm -rf /var/lib/apt/lists/*

    RUN zkg autoconfig --force
RUN zkg install --force salesforce/ja3
RUN apt-get clean

COPY ./zeek-entrypoint.sh /usr/local/bin/entrypoint.sh
RUN chmod +x /usr/local/bin/entrypoint.sh
ENTRYPOINT ["/usr/local/bin/entrypoint.sh"]
CMD ["zeek", "-C", "local"]