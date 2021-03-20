mkdir ssl
cd ssl
mkcert %FLUKE_GUN_PEER_CONTAINER_ID_1%.brazilsouth.azurecontainer.io
mkcert %FLUKE_GUN_PEER_CONTAINER_ID_2%.brazilsouth.azurecontainer.io
mkcert flukegunpeer-test.brazilsouth.azurecontainer.io
