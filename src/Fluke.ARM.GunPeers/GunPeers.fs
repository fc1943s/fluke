namespace Fluke.ARM.GunPeers

open System
open System.IO
open Farmer
open Farmer.Builders

module GunPeers =

    type Port = Port of int

    type FileShareId = FileShareId of id: string
    type ContainerId = ContainerId of id: string
    let serverPort = Port 8765

    /// A storage account, with a file share for the server config and world data.
    let rec gunstorage1 (FileShareId fileShareId) =
        storageAccount {
            name (nameof gunstorage1)
            sku Storage.Sku.Standard_LRS
            add_file_share_with_quota fileShareId 5<Gb>
        }


    /// A deployment script to create the config in the file share.
//    let rec deploymentScript1 shareName =
//        /// Helper function to base64 encode the files for embedding them in the deployment script.
//        let scriptSource = "echo a; echo b; echo c"
//
//        deploymentScript {
//            name (nameof deploymentScript1)
//            depends_on (gunstorage1 shareName)
//            script_content scriptSource
//            force_update
//        }


    let rec gunPeer (ContainerId containerId) (FileShareId fileShareId) =
        let rec ``share-fluke-gun-peer`` = nameof ``share-fluke-gun-peer``

        containerGroup {
            name containerId

            public_dns
                containerId
                [
                    let (Port port) = serverPort
                    TCP, uint16 port
                ]

            restart_policy ContainerGroup.AlwaysRestart

            // Add the file share for the world data and server configuration.
            add_volumes [
                volume_mount.secrets
                    ``share-fluke-gun-peer``
                    [
                        let projectDir = "Fluke.GunPeer"
                        "package.json", (File.ReadAllBytes $"../{projectDir}/package.json")
                        "server.js", (File.ReadAllBytes $"../{projectDir}/server.js")

                        "cert.pem",
                        (File.ReadAllBytes $"../{projectDir}/ssl/{containerId}.brazilsouth.azurecontainer.io.pem")

                        "key.pem",
                        (File.ReadAllBytes $"../{projectDir}/ssl/{containerId}.brazilsouth.azurecontainer.io-key.pem")
                    ]
                volume_mount.azureFile
                    fileShareId
                    fileShareId
                    (gunstorage1 (FileShareId fileShareId))
                        .Name
                        .ResourceName
                        .Value
            ]

            add_instances [
                containerInstance {
                    name (nameof containerInstance)
                    image "node:16.2-alpine"

                    env_vars [
                        "GUN_FILE", $"/data/{fileShareId}/{containerId}-radata"
                        "HTTPS_KEY", "/app/key.pem"
                        "HTTPS_CERT", "/app/cert.pem"
                    ]
                    // $"cd /data/{shareName}; while true; do java -Djava.net.preferIPv4Stack=true -Xms1G -Xmx3G -jar server.jar nogui && break; sleep 30; done"
                    // If we chose a custom port in the settings, it should go here.
                    add_public_ports [
                        let (Port port) = serverPort
                        uint16 port
                    ]
                    cpu_cores 1
                    memory 0.1<Gb>
                    // It needs a couple cores or the world may lag with a few players
                    //                    cpu_cores 2
                    // Give it enough memory for the JVM
                    //                    memory 3.5<Gb>
                    // Mount the path to the Azure Storage File share in the container
                    add_volume_mount fileShareId $"/data/{fileShareId}"
                    add_volume_mount ``share-fluke-gun-peer`` "/app"

                    // The command line needs to change to the directory for the file share and then start the server
                    // It needs a little more memory than the defaults, -Xmx3G gives it 3 GiB of memory.
                    command_line [
                        "/bin/sh"
                        "-c"
                        // We will need to do a retry loop since we can't have a depends_on for the deploymentScript to finish.
                        "cd /app; yarn install; while true; do yarn start && break; sleep 30; done"
                        //                        $"cd /data/{shareName}; while true; do java -Djava.net.preferIPv4Stack=true -Xms1G -Xmx3G -jar server.jar nogui && break; sleep 30; done"
                        ]
                }
            ]
        }

    let deployment =
        let fileShareId = FileShareId $"share-{nameof gunstorage1}"

        arm {
            location Location.EastUS

            add_resources [
                gunstorage1 fileShareId
                //                deploymentScript1 shareName
                gunPeer (ContainerId (Environment.GetEnvironmentVariable "FLUKE_GUN_PEER_CONTAINER_ID_1")) fileShareId
//                gunPeer (ContainerId (Environment.GetEnvironmentVariable "FLUKE_GUN_PEER_CONTAINER_ID_2")) fileShareId
//                gunPeer (ContainerId "flukegunpeer-test") fileShareId
            ]
        }
