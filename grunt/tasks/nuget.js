module.exports = function (grunt) {

  grunt.registerTask('nuget',
    'get nuget assemblies', function () {
      var
        //fs = require('fs'),
        //S = require('string'),
        string = require('string-formatter'),
        os = grunt.config('os'),
        config = grunt.config('config'),
        //configuration = grunt.config('msbuild_configuration'),
        nuget_path = os === 'win' ?
          config.win.nuget : config.linux.nuget,
        format_str = os === 'win' ?
          '{0} restore "{1}"' :
          'mono --runtime=v4.0.30319 {0} restore {1}',
            tasks = [];

      function getPackagesWithTitle(title) {
        var
          sln = string.format('./../{0}.sln', title),
          restore = string.format(format_str, nuget_path, sln);

        tasks.push(restore);
      }

      if (os === 'win') {
        getPackagesWithTitle('EngineIoClientDotNet');
      }

       grunt.log.writeln('tasks = %s', JSON.stringify(tasks));
       grunt.config('shell.exec.command', tasks.join('&&'));
       grunt.task.run('shell');
     });
};

