mkdir ssl
cd ssl
mkcert %FLUKE_GUN_PEER_CONTAINER_ID_1%.eastus.azurecontainer.io
mkcert %FLUKE_GUN_PEER_CONTAINER_ID_2%.eastus.azurecontainer.io
mkcert flukegunpeer-test.eastus.azurecontainer.io
