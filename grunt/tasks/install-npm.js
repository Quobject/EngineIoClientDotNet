module.exports = function (grunt) {

  grunt.registerTask('installNpm',
      'install node modules', function () {
        var 
          server_path = grunt.config('server_path'),
          os = grunt.config('os');

        grunt.log.writeln('server_path = "%s"', server_path);

        if (os === 'win') {
          grunt.config('shell.exec.options.execOptions.cwd', '<%= server_path %>');

          grunt.config('shell.exec.command', ['C:/WINDOWS/System32/WindowsPowerShell/v1.0/powershell.exe pwd',
            'npm install'].join('&&'));
          grunt.task.run('shell');

        } else {

          grunt.config('shell.exec.options.execOptions.cwd', '<%= server_path %>');
          grunt.config('shell.exec.command', ['pwd', 'npm install'].join('&&'));
          grunt.task.run('shell');
        }

      });
};


