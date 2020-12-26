const { spawn } = require("child_process");

if (process.env.GITHUB_JOB) {
  console.log('CI Build');
} else {
  spawn('patch-package', { shell: true, stdio: 'inherit' });
}
