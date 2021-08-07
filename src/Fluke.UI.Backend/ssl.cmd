mkdir ssl
cd ssl
mkcert %FLUKE_HUB_PEER_CONTAINER_ID_1%.eastus.azurecontainer.io
mkcert flukehubpeer-test.eastus.azurecontainer.io
