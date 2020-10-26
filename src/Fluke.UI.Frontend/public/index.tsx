import * as Gun from 'gun';

require('gun/sea');
require('gun/lib/promise');

interface AppState {
    users: { username: string }[],
    tasks: {
        name: string,
        cells?: { date: string, status: string, selected?: boolean }[]
    }[],
    trees: { position: string }[]
}

const createUser = async function (user, username, password, option = {}) {
    return (new Promise((res) => {
        user.create(username, password, res, option);
    }));
}
const authUser = async function (user, username, password, option = {}) {
    return (new Promise((res) => {
        user.auth(username, password, res, option);
    }));
}


window['login'] = async (username = 'fc1943s', password = 'pw1') => {
    const gun = Gun<AppState>(["http://localhost:8765/gun"]);
    const user = gun.user();
    console.log('user', user);
    const userData = await gun.get('~@' + username).promOnce();

    if (userData) {
        console.log('user already exists', userData);
    } else {
        const ack = await createUser(user, 'username', 'pw1');
        if (ack.err) {
            console.log("error creating user", ack);
        } else {
            console.log("create user ack", ack);
        }
    }
}
window['auth'] = async (username = 'fc1943s', password = 'pw1') => {
    const gun = Gun<AppState>(["http://localhost:8765/gun"]);
    const user = gun.user();
    console.log('user', user);

    const ack = await authUser(user, 'username', 'pw1');
    if (ack.err) {
        console.log("error authing user", ack);
    } else {
        console.log("auth user ack", ack);
    }
}


window['A'] = async () => {

    return;

    const users = gun.get("users");
    const testUser = users.set({username: "fluke"});

    const tasks = gun.get("tasks");

    const task1 = tasks.set({name: '01'});
    const cells1 = task1.get("cells");
    const cell1 = cells1.set({date: '2020-03-09', status: 'Disabled'});

    const task2 = tasks.set({name: '02'});
    const cells2 = task2.get("cells");
    const cell2 = cells2.set({date: '2020-03-09', status: 'Postponed None'});

    const event = {selected: true};

    const trees = gun.get("trees");
    trees.set({position: '2020-03-10 14:00'});
}
