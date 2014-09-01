
module.exports = function (grunt) {
  var
    node_os = require('os'),
    config = require('./config.json'),
    util = require('util'),
    os = node_os.platform() === 'win32' ? 'win' : 'linux';

  grunt.log.writeln(util.inspect(config));
  grunt.log.writeln( 'os = "%s"', os );

  grunt.loadTasks('./tasks');

  grunt.initConfig({      
    os: os,
    config: config,
   // msbuild_configuration: 'Debug',
    msbuild_configuration: 'Release',
    server_path: '../EngineIoClientDotNet_Tests/Resources',
    shell: {
      exec: {
        options: {
          stdout: true,
          stderr: true
        }
      }
    },
    jshint: {
      options: {
        jshintrc: true,
      },
      target: ['Gruntfile.js', '../EngineIoClientDotNet_Tests/Resources/server.js', 'tasks/**/*.js']
    }
  });

  
  grunt.loadNpmTasks('grunt-shell');
  grunt.loadNpmTasks('grunt-contrib-jshint');
  grunt.registerTask('default', ['jshint', 'installNpm', 'nuget', 'buildClient', 'buildTest', 'startServer', 'testClient']);
  grunt.registerTask('test', ['jshint', 'buildClient', 'buildTest', 'testClient']);
};
